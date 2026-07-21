using BloggingApp.Models;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace BloggingApp.Data
{
    public static class AdminUserInitializer
    {
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] Roles = ["Admin", "PostOwner", "User"];

            foreach(var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));
            }

            var adminEmail = "admin@gmail.com";

            var adminExist = await userManager.FindByEmailAsync(adminEmail);

            if (adminExist == null)
            {
                var user = new AppUser
                {
                    FullName = "System Administrator",
                    Email = adminEmail,
                    UserName = adminEmail,
                    //Role = "Admin",
                    Gender = "Male",
                    DateOfBirth = new DateOnly(2000, 01, 01),
                    Address = "Corporate Office, Madikeri - 571201",
                    PhoneNumber = "9999999999",
                    ProfilePictureUrl = "/profiles/admin.jpg",
                    EmailConfirmed = true
                };

                string adminPassword = "Admin@1234";
                var createResult = await userManager.CreateAsync(user, adminPassword);

                if (createResult.Succeeded)
                {
                    Debug.WriteLine("====== IDENTITY SEEDER SUCCESS: Admin seeded! ======");
                    await userManager.AddToRoleAsync(user, "Admin"); 
                }
                else
                {
                    var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                    Debug.WriteLine($"====== IDENTITY SEEDER ERROR: {errors} ======");
                    throw new Exception($"Failed to create new admin user : {errors}");
                }
            }

        }
    }
}
