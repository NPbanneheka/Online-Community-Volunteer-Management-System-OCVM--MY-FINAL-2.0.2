// Seeds default roles and the initial administrator account.
// Technology map:
// - ASP.NET Core Identity creates roles and admin user securely.
// - Dependency Injection provides UserManager and RoleManager.
// Connected files: Program.cs calls this service during application startup.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Services;

public static class DbInitializer
{
    // Service entry point used by Program.cs or controllers to prepare demo/runtime data.    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        string[] roles = ["Admin", "Organizer", "Volunteer"];

        foreach (var role in roles)
        {
            if (!// Check whether the required role already exists before creating it.
            await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "admin@gmail.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var result = // Create the default/demo Identity user account.
            await userManager.CreateAsync(adminUser, "123456");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Default admin account could not be created: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        adminUser.LockoutEnabled = true;
        await userManager.UpdateAsync(adminUser);

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        var adminProfile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == adminUser.Id);
        if (adminProfile == null)
        {
            context.UserProfiles.Add(new UserProfile
            {
                UserId = adminUser.Id,
                FullName = "System Administrator",
                RoleName = "Admin",
                PublicEmail = adminEmail,
                IsVerified = true,
                Bio = "Default administrator account for OCVMS."
            });

            await context.SaveChangesAsync();
        }
    }
}
