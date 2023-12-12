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
using abys_agrivet_backend.Helper.UsersProps;
using abys_agrivet_backend.Interfaces;
using abys_agrivet_backend.Repository.UsersRepository;
using abys_agrivet_backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using MailSettings = abys_agrivet_backend.Helper.MailSettings.MailSettings;

namespace abys_agrivet_backend.BusinessLayer.Logics;

public abstract class EntityFrameworkUsers<TEntity, TContext> : UsersRepository<TEntity>
where TEntity : class, IUsers
where TContext : APIDBContext
{
    private readonly TContext context;
    private readonly UserManager<JWTIdentity> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private IConfiguration _configuration;
    private APIDBContext context1;
    private UserManager<JWTIdentity> userManager;
    private RoleManager<IdentityRole> roleManager;
    private IConfiguration configuration;
    private readonly MailSettings _mailSettings;

    public EntityFrameworkUsers(
        TContext context,
        UserManager<JWTIdentity> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration, IOptions<MailSettings> mailSettings)
    {
        this.context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _mailSettings = mailSettings.Value;
    }

    protected EntityFrameworkUsers(APIDBContext context1, UserManager<JWTIdentity> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        this.context1 = context1;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.configuration = configuration;
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
        // var smsProvider = new SMSTwilioService();
        var findBranchId = await context.Branches
        .Where(x => x.branchKey == "all").FirstOrDefaultAsync();

        if (findBranchId != null)
        {
            string mightHashPassword = BCrypt.Net.BCrypt.HashPassword(entity.password);
            entity.password = mightHashPassword;
            entity.access_level = 1;
            entity.branch = findBranchId.branch_id;
            entity.status = Convert.ToChar("1");
            entity.phoneNumber = entity.phoneNumber;
            entity.imgurl = "no-image-found";
            entity.verified = Convert.ToChar("1");
            entity.created_at = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
            entity.updated_at = Convert.ToDateTime(System.DateTime.Now.ToString("MM/dd/yyyy"));
            // smsProvider.SendSMSService("You've successfully registered on the application", entity.phoneNumber);
            await SendWelcomeEmailSMTPWithoutCode(entity.email, "You've successfully registered on the application");
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }
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
        int[] branches = new[] { 1, 2, 3, 4, 5, 6 };
        return context.Set<TEntity>().Where(x => branches.Contains(x.branch)).ToList();
    }

    public async Task<dynamic> AccountSigningIn(LoginParameters loginParameters)
    {

        var user = await _userManager.FindByNameAsync(loginParameters.email);
        var findBranchIdByEmail =
            await context.Set<TEntity>().Where(x => x.email == loginParameters.email).FirstOrDefaultAsync();
        bool lookUpUserBasedOnBranch = await context.Set<TEntity>()
            .AnyAsync(x => x.email == loginParameters.email && x.branch == findBranchIdByEmail.branch);

        var lookUpAllUserBasedEmailDefault = await context.Set<TEntity>()
            .Where(x => x.email == loginParameters.email && x.branch == findBranchIdByEmail.branch).FirstOrDefaultAsync();
        var checkBranchFromSource = await context.Branches.AnyAsync(x =>
            x.branch_id == findBranchIdByEmail.branch && x.branchStatus == Convert.ToChar("1"));
        var FindPathOnBranches = await
            context.Branches.Where(x => x.branch_id == findBranchIdByEmail.branch).FirstOrDefaultAsync();

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
                            if (lookUpAllUserBasedEmailDefault.branch == findBranchIdByEmail.branch)
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
                if (lookUpAllUserBasedEmailDefault.access_level == 3 &&
                lookUpAllUserBasedEmailDefault.branch == 0)
                {
                    if (lookUpAllUserBasedEmailDefault.status == Convert.ToChar("1"))
                    {
                        if (BCrypt.Net.BCrypt.Verify(loginParameters.password, encryptedPassword))
                        {
                            if (lookUpAllUserBasedEmailDefault.branch == findBranchIdByEmail.branch)
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
                                    obj.branchPath = "/customer/dashboard";
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
                    return "BRANCH_NOT_WORKING";
                }
            }
        }

        return 200;
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
        if (core_user == null) throw new ArgumentNullException(nameof(core_user));
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

    public async Task<dynamic> DeleteUser(int id)
    {
        var uamDeleteEntity = await context.Set<TEntity>().Where(x => x.id == id).FirstOrDefaultAsync();
        if (uamDeleteEntity != null)
        {
            context.Set<TEntity>().Remove(uamDeleteEntity);
            await context.SaveChangesAsync();
            return 200;
        }

        return 401;
    }

    public async Task<dynamic> UpdateProfile(UsersParameters entity)
    {

        var changeSomeDetailsFromUser =
            await context.UsersEnumerable.Where(x => x.email == entity.email && x.id == entity.id).FirstOrDefaultAsync();
        var checkEmailRequestToChange = await context.Set<TEntity>()
            .AnyAsync(x => x.email == entity.email && x.id == entity.id);

        if (changeSomeDetailsFromUser.email == entity.email)
        {
            if (BCrypt.Net.BCrypt.Verify(entity.password, changeSomeDetailsFromUser.password))
            {
                changeSomeDetailsFromUser.firstname = entity.firstname;
                changeSomeDetailsFromUser.middlename = entity.middlename;
                changeSomeDetailsFromUser.lastname = entity.lastname;
                changeSomeDetailsFromUser.phoneNumber = entity.phoneNumber;
                changeSomeDetailsFromUser.username = entity.username;
                changeSomeDetailsFromUser.imgurl = entity.imgurl;
                var core_user = await _userManager.FindByEmailAsync(entity.email);
                if (core_user == null) throw new ArgumentNullException(nameof(core_user));
                if (entity.newPassword != null)
                {
                    changeSomeDetailsFromUser.password = BCrypt.Net.BCrypt.HashPassword(entity.newPassword);
                }
                var code = await _userManager.GeneratePasswordResetTokenAsync(core_user);
                var result = await _userManager.ResetPasswordAsync(core_user, code, entity.password);
                if (!result.Succeeded)
                    return 401;
                await context.SaveChangesAsync();
                dynamic obj = new ExpandoObject();
                obj.status = 200;
                obj.forReferences = changeSomeDetailsFromUser;
                return obj;
            }
            else
            {
                return 400;
            }
        }
        else
        {
            if (checkEmailRequestToChange)
            {
                return 409;
            }
            else
            {
                if (BCrypt.Net.BCrypt.Verify(entity.password, changeSomeDetailsFromUser.password))
                {
                    changeSomeDetailsFromUser.firstname = entity.firstname;
                    changeSomeDetailsFromUser.middlename = entity.middlename;
                    changeSomeDetailsFromUser.lastname = entity.lastname;
                    changeSomeDetailsFromUser.phoneNumber = entity.phoneNumber;
                    changeSomeDetailsFromUser.username = entity.username;
                    changeSomeDetailsFromUser.imgurl = entity.imgurl;
                    var core_user = await _userManager.FindByEmailAsync(entity.email);
                    if (core_user == null) throw new ArgumentNullException(nameof(core_user));
                    if (entity.newPassword != null)
                    {
                        changeSomeDetailsFromUser.password = BCrypt.Net.BCrypt.HashPassword(entity.newPassword);
                    }
                    var code = await _userManager.GeneratePasswordResetTokenAsync(core_user);
                    var result = await _userManager.ResetPasswordAsync(core_user, code, entity.password);
                    if (!result.Succeeded)
                        return 401;
                    await context.SaveChangesAsync();
                    dynamic obj = new ExpandoObject();
                    obj.status = 201;
                    obj.forReferences = changeSomeDetailsFromUser;
                    return obj;
                }
                else
                {
                    return 400;
                }
            }
        }
    }

    public async Task<List<TEntity>> FilterByAccessLevel(int access_level)
    {
        var accessLevelFiltered = await context.Set<TEntity>().Where(x => x.access_level == access_level).ToListAsync();
        return accessLevelFiltered;
    }

    class ForgeBranches
    {
        public int branch_id { get; set; }
        public string serviceBranch { get; set; }
    }
    public async Task<List<Model.Services>> FilterServicesByBranch(int branch_id)
    {
        var entityToBeUpdate = await context.ServicesEnumerable.Where(x => x.serviceStatus == 1)
            .ToListAsync();
        for (int x = 0; x < entityToBeUpdate.Count; x++)
        {
            Model.Services forges = entityToBeUpdate[x];
            foreach (var entity in entityToBeUpdate)
            {
                List<ForgeBranches> forgeEntity = JsonSerializer.Deserialize<List<ForgeBranches>>(forges.serviceBranch);
                var listServices = await context.ServicesEnumerable.Where(x => forgeEntity.Select(mc => mc.branch_id).Contains(branch_id)).ToListAsync();
                return listServices;
            }
        }
        return entityToBeUpdate;
    }

    public async Task SendEmailSMTPWithCode(string email, string code, string? body)
    {
        /*string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\emailTemplate.html";
        StreamReader str = new StreamReader(FilePath);
        string MailText = str.ReadToEnd();
        str.Close();
        MailText = MailText.Replace("[username]", "User").Replace("[email]", email).Replace("[verificationCode]", code)
            .Replace("[body]", body);
        var mail = new MimeMessage();
        mail.Sender = MailboxAddress.Parse(_mailSettings.Mail);
        mail.To.Add(MailboxAddress.Parse(email));
        mail.Subject = $"Welcome {email}";
        var builder = new BodyBuilder();
        builder.HtmlBody = MailText;
        mail.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(mail);
        smtp.Disconnect(true);*/
        var apiKey = "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";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("agrivetabys@gmail.com", "Abys Agrivet System");
        var subject = "Abys Agrivet Notification";
        var to = new EmailAddress(email, "User");
        var plainTextContent = "ABYS AGRIVET NOTIFICATIONS";
        var htmlContent = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>OTP Email</title>
</head>
<body>
    <div style=""text-align: center;"">
        <h1>Your OTP Code</h1>
        <p>Use the following code to verify your account:</p>
        <h2 style=""color: #007bff;"">" + code + @"</h2>
    </div>
</body>
</html>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        await client.SendEmailAsync(msg);
    }
    public async Task SendWelcomeEmailSMTPWithoutCode(string email, string? body)
    {
        var apiKey = "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";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("agrivetabys@gmail.com", "Abys Agrivet System");
        var subject = "Abys Agrivet Notification";
        var to = new EmailAddress(email, "User");
        var plainTextContent = "ABYS AGRIVET NOTIFICATIONS";
        var htmlContent = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>OTP Email</title>
</head>
<body>
    <div style=""text-align: center;"">
        <h2 style=""color: #007bff;"">" + body + @"</h2>
    </div>
</body>
</html>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        await client.SendEmailAsync(msg);
    }
}