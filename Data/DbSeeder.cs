using ECommerceApplication.Utilities;
using ECommerceApplication.Models;
using Microsoft.AspNetCore.Identity;

namespace ECommerceApplication.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] defaultRoles = [
                Roles.Admin,
                Roles.Customer,
                Roles.Vendor
                ];
            foreach (var role in defaultRoles)
            {
                // check if not exist role
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }


            // Seed admin user
            const string adminEmail = "admin@ecommerce.com";
            const string adminPassword = "Admin@123";
            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Shop",
                    LastName = "Admin",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin,adminPassword );
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
        }
}
}
