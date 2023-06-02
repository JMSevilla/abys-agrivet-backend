using abys_agrivet_backend.Helper.JWT;
using abys_agrivet_backend.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace abys_agrivet_backend.DB;

public class APIDBContext : IdentityDbContext<JWTIdentity>
{
    //
    public APIDBContext(DbContextOptions<APIDBContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        //builder.Ignore<Users>();
        //builder.Ignore<Branch>();
    }
    public DbSet<Users> UsersEnumerable { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Verification> Verifications { get; set; }
}