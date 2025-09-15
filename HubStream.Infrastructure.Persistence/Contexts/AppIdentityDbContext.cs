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
                b.Property(u => u.Id)
                 .HasConversion(identifierConverter)
                 .HasMaxLength(50)
                 .HasColumnType("varchar(50)");
            });

            // Roles
            builder.Entity<ApplicationRole>(b =>
            {
                b.Property(r => r.Id)
                 .HasConversion(identifierConverter)
                 .HasMaxLength(50)
                 .HasColumnType("varchar(50)");
            });

            // Relaciones
            builder.Entity<ApplicationUserClaim>(b => b.Property(uc => uc.UserId).HasConversion(identifierConverter));
            builder.Entity<ApplicationUserRole>(b =>
            {
                b.Property(ur => ur.UserId).HasConversion(identifierConverter);
                b.Property(ur => ur.RoleId).HasConversion(identifierConverter);
            });
            builder.Entity<ApplicationUserLogin>(b => b.Property(ul => ul.UserId).HasConversion(identifierConverter));
            builder.Entity<ApplicationRoleClaim>(b => b.Property(rc => rc.RoleId).HasConversion(identifierConverter));
            builder.Entity<ApplicationUserToken>(b => b.Property(ut => ut.UserId).HasConversion(identifierConverter));
        }
    }
}
