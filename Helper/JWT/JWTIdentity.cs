using Microsoft.AspNetCore.Identity;

namespace abys_agrivet_backend.Helper.JWT;

public class JWTIdentity : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}