using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.ForgotPassword;
using abys_agrivet_backend.Helper.JWT;
using abys_agrivet_backend.Helper.JWTResponse;
using abys_agrivet_backend.Helper.LoginParams;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.UsersRepository;
using abys_agrivet_backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace abys_agrivet_backend.BusinessLayer.Logics;

public abstract class EntityFrameworkUsers<TEntity, TContext> : UsersRepository<TEntity>
where TEntity : class, IUsers
where TContext : APIDBContext
{
    private readonly TContext context;
    private readonly UserManager<JWTIdentity> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private IConfiguration _configuration;

    public EntityFrameworkUsers(
        TContext context,
        UserManager<JWTIdentity> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        this.context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<bool> SetupApplicationFindAnyUsersFromDB()
    {
        int checkUsersInDB = await context.Set<TEntity>().CountAsync();
        if (checkUsersInDB > 0)
        {
            return true;
        }

        return false;
    }

    public async Task<TEntity> SetupAccountFirstUserOfTheApplication(TEntity entity)
    {
        var smsProvider = new SMSTwilioService();
        string mightHashPassword = BCrypt.Net.BCrypt.HashPassword(entity.password);
        entity.password = mightHashPassword;
        entity.access_level = 1;
        entity.branch = 6;
        entity.status = Convert.ToChar("1");
        entity.phoneNumber = entity.phoneNumber;
        entity.imgurl = "no-image-found";
        entity.verified = Convert.ToChar("1");
        entity.created_at = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
        entity.updated_at = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
        smsProvider.SendSMSService("You've successfully registered on the application", entity.phoneNumber);
        context.Set<TEntity>().Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<dynamic> UAM(TEntity entity)
    {
        var checkUserIfExists = await context.Set<TEntity>().AnyAsync(x => x.email == entity.email);
        var userExists = await _userManager.FindByEmailAsync(entity.email);
        
        if (checkUserIfExists)
        {
            return "user_exists";
        }
        else
        {
            if (userExists != null)
                return "jwt_user_exists";
            JWTIdentity user = new()
            {
                Email = entity.email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = entity.email
            };
            var result = await _userManager.CreateAsync(user, entity.password);
            if (!result.Succeeded)
                return "Password is too weak";
            string mightHashPassword = BCrypt.Net.BCrypt.HashPassword(entity.password);
            entity.password = mightHashPassword;
            entity.access_level = entity.access_level;
            entity.branch = entity.branch;
            entity.status = Convert.ToChar("1");
            entity.imgurl = "no-image-found";
            entity.verified = Convert.ToChar("1");
            entity.created_at = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
            entity.updated_at = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync();
            return new JWTResponse { Status = "Success", Message = "User created successfully" };
        }
    }

    public List<TEntity> UAMGetAll()
    {
        return context.Set<TEntity>().Where(x => x.status == Convert.ToChar("1")).ToList();
    }

    public async Task<dynamic> AccountSigningIn(LoginParameters loginParameters)
    {
        if (loginParameters.accountType == "employee")
        {
            var user = await _userManager.FindByNameAsync(loginParameters.email);
        bool lookUpUserBasedOnBranch = await context.Set<TEntity>()
            .AnyAsync(x => x.email == loginParameters.email && x.branch == loginParameters.branch);
        
        var lookUpAllUserBasedEmailDefault = await context.Set<TEntity>()
            .Where(x => x.email == loginParameters.email && x.branch == loginParameters.branch).FirstOrDefaultAsync();
        var checkBranchFromSource = await context.Branches.AnyAsync(x =>
            x.branch_id == loginParameters.branch && x.branchStatus == Convert.ToChar("1"));
        var FindPathOnBranches = await
            context.Branches.Where(x => x.branch_id == loginParameters.branch).FirstOrDefaultAsync();
        dynamic obj = new ExpandoObject();
        if (string.IsNullOrWhiteSpace(loginParameters.email) || string.IsNullOrWhiteSpace(loginParameters.password))
        {
            return "EMPTY";
        }
        else
        {
            string encryptedPassword =
                lookUpAllUserBasedEmailDefault == null ? "" : lookUpAllUserBasedEmailDefault.password;
            if (checkBranchFromSource)
            {
                if (lookUpUserBasedOnBranch)
            {
                if (lookUpAllUserBasedEmailDefault.status == Convert.ToChar("1"))
                {
                    if (BCrypt.Net.BCrypt.Verify(loginParameters.password, encryptedPassword))
                    {
                        if (lookUpAllUserBasedEmailDefault.branch == loginParameters.branch)
                        {
                            if (user != null && await _userManager.CheckPasswordAsync(user, loginParameters.password))
                            {
                                var userRoles = await _userManager.GetRolesAsync(user);
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, user.UserName),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                                };
                                foreach (var userRole in userRoles)
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                                }

                                var token = CreateToken(claims);
                                var refreshToken = GenerateRefreshToken();

                                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"],
                                    out int refreshTokenValidityInDays);
                                user.RefreshToken = refreshToken;
                                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
                                await _userManager.UpdateAsync(user);
                                obj.TokenInfo = new
                                {
                                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                                    RefreshToken = refreshToken,
                                    Expiration = token.ValidTo
                                };
                                obj.status = "SUCCESS";
                                obj.branchPath = FindPathOnBranches.branchPath;
                                obj.usertype = lookUpAllUserBasedEmailDefault.access_level;
                                obj.uid = lookUpAllUserBasedEmailDefault.id;
                                obj.references = lookUpAllUserBasedEmailDefault;
                                return obj;
                            }

                            return "UNAUTHORIZED";
                        }
                    }
                    else
                    {
                        return "INVALID_PASSWORD";
                    }
                }
                else
                {
                    return "ACCOUNT_DISABLED";
                }
            }
            else
            {
                return "ACCOUNT_NOT_EXISTS_ON_THIS_BRANCH";
            }
            }
            else
            {
                return "BRANCH_NOT_WORKING";
            }
        }
        }
        else
        {
            // Customer / Client
              var user = await _userManager.FindByNameAsync(loginParameters.email);
              
        var lookUpAllUserBasedEmailDefault = await context.Set<TEntity>()
            .Where(x => x.email == loginParameters.email ).FirstOrDefaultAsync();
        
        dynamic obj = new ExpandoObject();
        if (string.IsNullOrWhiteSpace(loginParameters.email) || string.IsNullOrWhiteSpace(loginParameters.password))
        {
            return "EMPTY";
        }
        else
        {
            string encryptedPassword =
                lookUpAllUserBasedEmailDefault == null ? "" : lookUpAllUserBasedEmailDefault.password;
                if (lookUpAllUserBasedEmailDefault.status == Convert.ToChar("1"))
                {
                    if (BCrypt.Net.BCrypt.Verify(loginParameters.password, encryptedPassword))
                    {
                        if (lookUpAllUserBasedEmailDefault.branch == loginParameters.branch)
                        {
                            if (user != null && await _userManager.CheckPasswordAsync(user, loginParameters.password))
                            {
                                var userRoles = await _userManager.GetRolesAsync(user);
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, user.UserName),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                                };
                                foreach (var userRole in userRoles)
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                                }

                                var token = CreateToken(claims);
                                var refreshToken = GenerateRefreshToken();

                                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"],
                                    out int refreshTokenValidityInDays);
                                user.RefreshToken = refreshToken;
                                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
                                await _userManager.UpdateAsync(user);
                                obj.TokenInfo = new
                                {
                                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                                    RefreshToken = refreshToken,
                                    Expiration = token.ValidTo
                                };
                                obj.status = "SUCCESS";
                                obj.usertype = lookUpAllUserBasedEmailDefault.access_level;
                                obj.uid = lookUpAllUserBasedEmailDefault.id;
                                obj.references = lookUpAllUserBasedEmailDefault;
                                return obj;
                            }

                            return "UNAUTHORIZED";
                        }
                        else
                        {
                            return "NO_ACCOUNT_ON_THIS_BRANCH";
                        }
                    }
                    else
                    {
                        return "INVALID_PASSWORD";
                    }
                }
                else
                {
                    return "ACCOUNT_DISABLED";
                }
            
        }
        }
        return "SOMETHING_WENT_WRONG";
    }

    public JwtSecurityToken CreateToken(List<Claim> claims)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:IssuerSigningKey"]));
        _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
            claims: claims,
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        );
        return token;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<dynamic> RefreshToken(AccessWithRefresh accessWithRefresh)
    {
        dynamic obj = new ExpandoObject();
        if (accessWithRefresh is null)
        {
            return "Invalid client request";
        }

        string? accessToken = accessWithRefresh.AccessToken;
        string? refreshToken = accessWithRefresh.RefreshToken;

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
        {
            return "Invalid access token or refresh token";
        }

        string username = principal.Identity.Name;
        var user = await _userManager.FindByNameAsync(username);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        {
            return "Invalid access token or refresh token";
        }

        var newAccessToken = CreateToken(principal.Claims.ToList());
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        await _userManager.UpdateAsync(user);
        obj.accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken);
        obj.refreshToken = newRefreshToken;
        return obj;
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:IssuerSigningKey"])),
            ValidateLifetime = false
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        return principal;
    }

    public async Task<dynamic> CustomerAccountRegistration(TEntity entity)
    {
        var checkExistingAccount = await context.Set<TEntity>().AnyAsync(x => x.email == entity.email);
        if (checkExistingAccount)
        {
            return "account_exist";
        }
        else
        {
            JWTCredentials credentials = new()
            {
                jwtusername = entity.email,
                jwtpassword = entity.password
            };
            await JWTAccountCreation(credentials);
            string mightHashPassword = BCrypt.Net.BCrypt.HashPassword(entity.password);
            entity.password = mightHashPassword;
            entity.access_level = 3;
            entity.branch = 0;
            entity.status = Convert.ToChar("1");
            entity.phoneNumber = entity.phoneNumber;
            entity.imgurl = "no-image-found";
            entity.verified = Convert.ToChar("0");
            entity.created_at = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
            entity.updated_at = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
            
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync();
            return 200;
        }
    }

    public async Task<dynamic> JWTAccountCreation(JWTCredentials jwtCredentials)
    {
        var userExists = await _userManager.FindByEmailAsync(jwtCredentials.jwtusername);
        if (userExists != null)
            return "User already exists";
        JWTIdentity user = new()
        {
            Email = jwtCredentials.jwtusername,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = jwtCredentials.jwtusername
        };
        var result = await _userManager.CreateAsync(user, jwtCredentials.jwtpassword);
        if (!result.Succeeded)
            return "something_wrong_creating_account";
        return "Success";
    }

    public async Task<dynamic> ReUsableCheckingEmail(string email)
    {
        var result = await context.Set<TEntity>().AnyAsync(x => x.email == email);
        if (result)
        {
            return "exist";
        }
        else
        {
            return "not_exist";
        }
    }

    public async Task<dynamic> ChangePassword(ForgotPasswordParams forgotPasswordParams)
    {
        var userIdentifierIfAny = await context.Set<TEntity>()
            .AnyAsync(x => x.email == forgotPasswordParams.email);
        var changePasswordEntity = await context.Set<TEntity>()
            .Where(x => x.email == forgotPasswordParams.email).FirstOrDefaultAsync();
        var core_user = await _userManager.FindByEmailAsync(forgotPasswordParams.email);
        if (userIdentifierIfAny)
        {
            changePasswordEntity.password = BCrypt.Net.BCrypt.HashPassword(forgotPasswordParams.password);
            var code = await _userManager.GeneratePasswordResetTokenAsync(core_user);
            var result = await _userManager.ResetPasswordAsync(core_user, code, forgotPasswordParams.password);
            if (!result.Succeeded)
                return 401;
            await context.SaveChangesAsync();
            return 200;
        }

        return 401;
    }
}