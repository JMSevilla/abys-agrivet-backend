using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using abys_agrivet_backend.Helper.JWT;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using abys_agrivet_backend.Helper.JWTResponse;
using Microsoft.IdentityModel.Tokens;

namespace abys_agrivet_backend.Controllers.Authentication;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<JWTIdentity> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    public AuthenticationController(
        UserManager<JWTIdentity> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [Route("jwt-account-creation"), HttpPost]
    public async Task<IActionResult> JWTAccountCreation([FromBody] JWTCredentials jwtCredentials)
    {
        var userExists = await _userManager.FindByEmailAsync(jwtCredentials.jwtusername);
        if (userExists != null)
            return StatusCode(StatusCodes.Status500InternalServerError, new JWTResponse{Status = "Error", Message = "User already exists"});
        JWTIdentity user = new()
        {
            Email = jwtCredentials.jwtusername,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = jwtCredentials.jwtusername
        };
        var result = await _userManager.CreateAsync(user, jwtCredentials.jwtpassword);
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError, new JWTResponse {Status = "Error", Message = "Theres something wrong creating the user"});
        return Ok(new JWTResponse { Status = "Success", Message = "User created successfully" });
    }

    [Route("jwt-account-login"), HttpPost]
    public async Task<IActionResult> JWTAccountLogin([FromBody] JWTCredentials jwtCredentials)
    {
        var user = await _userManager.FindByNameAsync(jwtCredentials.jwtusername);
        if (user != null && await _userManager.CheckPasswordAsync(user, jwtCredentials.jwtpassword))
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

            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = token.ValidTo
            });
            
        }

        return Unauthorized();
    }

    private JwtSecurityToken CreateToken(List<Claim> claims)
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

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}