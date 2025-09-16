using HubStream.Domain.Users.Entities;
using HubStream.Shared.Kernel.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Infrastructure.Persistence.Contexts
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using HubStream.Domain.Users.Entities;

    public class AppIdentityDbContext : IdentityDbContext<
     ApplicationUser,
     ApplicationRole,
     Identifier,
     ApplicationUserClaim,
     ApplicationUserRole,
     ApplicationUserLogin,
     ApplicationRoleClaim,
     ApplicationUserToken>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var identifierConverter = new ValueConverter<Identifier, string>(
                v => v.ToString(),
                v => Identifier.Parse(v)
            );

            // Users
            builder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("Users"); // 👈 renombramos tabla
                b.Property(u => u.Id)
                 .HasConversion(identifierConverter)
                 .HasMaxLength(50)
                 .HasColumnType("varchar(50)");
            });

            // Roles
            builder.Entity<ApplicationRole>(b =>
            {
                b.ToTable("Roles"); // 👈 renombramos tabla
                b.Property(r => r.Id)
                 .HasConversion(identifierConverter)
                 .HasMaxLength(50)
                 .HasColumnType("varchar(50)");
            });

            // UserClaims
            builder.Entity<ApplicationUserClaim>(b =>
            {
                b.ToTable("UserClaims"); // 👈
                b.Property(uc => uc.UserId).HasConversion(identifierConverter);
            });

            // UserRoles
            builder.Entity<ApplicationUserRole>(b =>
            {
                b.ToTable("UserRoles"); // 👈
                b.Property(ur => ur.UserId).HasConversion(identifierConverter);
                b.Property(ur => ur.RoleId).HasConversion(identifierConverter);
            });

            // UserLogins
            builder.Entity<ApplicationUserLogin>(b =>
            {
                b.ToTable("UserLogins"); // 👈
                b.Property(ul => ul.UserId).HasConversion(identifierConverter);
            });

            // RoleClaims
            builder.Entity<ApplicationRoleClaim>(b =>
            {
                b.ToTable("RoleClaims"); // 👈
                b.Property(rc => rc.RoleId).HasConversion(identifierConverter);
            });

            // UserTokens
            builder.Entity<ApplicationUserToken>(b =>
            {
                b.ToTable("UserTokens"); // 👈
                b.Property(ut => ut.UserId).HasConversion(identifierConverter);
            });
        }

    }
}
