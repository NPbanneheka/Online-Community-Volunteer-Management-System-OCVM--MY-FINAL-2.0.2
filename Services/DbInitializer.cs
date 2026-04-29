using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Services;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        string[] roles = ["Admin", "Organizer", "Volunteer"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
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

            var result = await userManager.CreateAsync(adminUser, "123456");
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
