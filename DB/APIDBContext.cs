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
        // builder.Ignore<Users>();
        // builder.Ignore<Branch>();
        // builder.Ignore<Verification>();
        // builder.Ignore<Model.Services>();
        // builder.Ignore<Schedule>();
        // builder.Ignore<Appointment>();
    }
    public DbSet<Users> UsersEnumerable { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Verification> Verifications { get; set; }
    public DbSet<Model.Services> ServicesEnumerable { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<FollowUpAppointment> FollowUpAppointments { get; set; }
}