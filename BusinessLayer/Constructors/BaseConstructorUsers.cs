using abys_agrivet_backend.BusinessLayer.Logics;
using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.JWT;
using abys_agrivet_backend.Helper.MailSettings;
using abys_agrivet_backend.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace abys_agrivet_backend.BusinessLayer.Constructors;

public class BaseConstructorUsers : EntityFrameworkUsers<Users, APIDBContext>
{
    public BaseConstructorUsers(APIDBContext context, UserManager<JWTIdentity> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IOptions<MailSettings> mailSettings) : base(context, userManager, roleManager, configuration, mailSettings) { }
}