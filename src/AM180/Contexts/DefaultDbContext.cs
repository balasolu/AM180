using AM180.Models.Abstractions;
using AM180.Models.Enums;
using AM180.Models.Roles;
using AM180.Models.Tokens;
using AM180.Models.Users;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AM180.Contexts;

public class DefaultDbContext : IdentityDbContext<User, Role, string>, IDataProtectionKeyContext
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options)
        : base(options) { }

    new public virtual DbSet<User>? Users { get; set; }
    new public virtual DbSet<Role>? Roles { get; set; }
    public virtual DbSet<Token>? Tokens { get; set; }
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public virtual DbSet<DataProtectionKey>? DataProtectionKeys { get; set; }
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var keysProperties = modelBuilder.Model.GetEntityTypes().Select(x => x.FindPrimaryKey()).SelectMany(x => x.Properties);

        foreach (var property in keysProperties)
            property.ValueGenerated = ValueGenerated.OnAdd;


        modelBuilder.Entity<IdentityRoleClaim<string>>()
            .ToTable("RoleClaims");

        modelBuilder.Entity<Role>()
            .ToTable("Roles")
            .HasDiscriminator<RoleType>(nameof(RoleType))
            .HasValue<DefaultRole>(RoleType.Default)
            .IsComplete();

        modelBuilder.Entity<IdentityUserClaim<string>>()
            .ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>()
            .ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserRole<string>>()
            .ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserToken<string>>()
            .ToTable("UserTokens");

        modelBuilder.Entity<Token>()
            .ToTable("Tokens")
            .HasDiscriminator<TokenType>(nameof(TokenType))
            .HasValue<DefaultToken>(TokenType.Default)
            .HasValue<AuthenticationToken>(TokenType.Authentication)
            .HasValue<RefreshToken>(TokenType.Refresh)
            .HasValue<ConfirmationToken>(TokenType.Confirmation)
            .IsComplete();

        modelBuilder.Entity<User>()
            .ToTable("Users")
            .HasDiscriminator<UserType>(nameof(UserType))
            .HasValue<DefaultUser>(UserType.Default)
            //.HasValue<EntertainerUser>(UserType.Vendor)
            .IsComplete();

    }
}
