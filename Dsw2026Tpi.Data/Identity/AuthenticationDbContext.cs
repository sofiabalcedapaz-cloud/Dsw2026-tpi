using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Data.Identity;

public class AuthenticationDbContext: IdentityDbContext<ApplicationUser>
{
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options)
            : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UsersRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UsersClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UsersLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RolesClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UsersTokens");
    }
}
