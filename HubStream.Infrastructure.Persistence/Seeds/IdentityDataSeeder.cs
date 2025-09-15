using HubStream.Domain.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubStream.Infrastructure.Persistence.Seeds
{
    using HubStream.Domain.Users.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;

    public static class IdentityDataSeeder
    {
        /// <summary>
        /// Seeds the database with initial roles and a default admin user.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies.</param>
        /// <param name="configuration">The application configuration to access settings.</param>
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            // 1. Resolve the required services from the dependency injection container.
            //    It is crucial to use the custom ApplicationUser and ApplicationRole types here.
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // 2. Define and seed the default roles for the application.
            await SeedRolesAsync(roleManager);

            // 3. Seed the default administrator user.
            await SeedAdminUserAsync(userManager, configuration);
        }

        /// <summary>
        /// Creates the default roles if they do not already exist.
        /// </summary>
        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            // Check if the "Admin" role exists.
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                // If not, create it.
                await roleManager.CreateAsync(new ApplicationRole { Id = Identifier.New(), Name = "Admin" });
            }

            // Check if the "User" role exists.
            if (!await roleManager.RoleExistsAsync("User"))
            {
                // If not, create it.
                await roleManager.CreateAsync(new ApplicationRole { Id = Identifier.New(), Name = "User" });
            }
        }

        /// <summary>
        /// Creates a default administrator user if one does not already exist.
        /// </summary>
        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            // Get the admin user's details from configuration (appsettings.json).
            // This avoids hardcoding sensitive credentials in the source code.
            string adminEmail = configuration["Identity:Admin:Email"];
            string adminPassword = configuration["Identity:Admin:Password"];

            // Validate that the configuration values are present.
            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                // You might want to log a warning here if the configuration is missing.
                return;
            }

            // Check if a user with the admin email already exists.
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                // Create a new ApplicationUser instance for the admin.
                var adminUser = new ApplicationUser
                {
                    Id = Identifier.New(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // Automatically confirm the email for the seed user.
                };

                // Create the user with the specified password.
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                // If the user was created successfully, assign them to the "Admin" role.
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }

}
