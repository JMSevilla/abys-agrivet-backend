using abys_agrivet_backend.BusinessLayer.Logics;
using abys_agrivet_backend.DB;
using abys_agrivet_backend.Helper.JWT;
using abys_agrivet_backend.Model;
using Microsoft.AspNetCore.Identity;

namespace abys_agrivet_backend.BusinessLayer.Constructors;

public class BaseConstructorUsers : EntityFrameworkUsers<Users, APIDBContext>
{
    public BaseConstructorUsers(APIDBContext context, UserManager<JWTIdentity> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration) : base(context, userManager, roleManager, configuration) {}
}