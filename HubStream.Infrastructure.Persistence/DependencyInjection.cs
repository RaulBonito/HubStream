using HubStream.Application.Services.Common;
using HubStream.Domain.Users.Entities;
using HubStream.Infrastructure.Persistence.Contexts;
using HubStream.Infrastructure.Persistence.Security;
// Elimina using Microsoft.AspNetCore.Authentication.JwtBearer; si ya no se usa
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// Elimina using Microsoft.IdentityModel.Tokens; si ya no se usa
// Elimina using System.Text; si ya no se usa

namespace HubStream.Infrastructure.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("IdentityConnection");

            // DbContext
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseMySql(
                    conn,
                    new MySqlServerVersion(new Version(8, 0, 36))
                ));

            // Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders()
            .AddRoles<ApplicationRole>()
            .AddSignInManager();

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}