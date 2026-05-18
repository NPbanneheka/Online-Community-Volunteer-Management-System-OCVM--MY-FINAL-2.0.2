<<<<<<< HEAD
// Services/DbInitializer.cs
// This service file that contains reusable setup or seeding logic used by the application.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core Identity services for users, roles, passwords, sign-in sessions, and lockout/ban behavior.
=======
// Seeds default roles and the initial administrator account.
// Technology map:
// - ASP.NET Core Identity creates roles and admin user securely.
// - Dependency Injection provides UserManager and RoleManager.
// Connected files: Program.cs calls this service during application startup.

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
using Microsoft.AspNetCore.Identity;
// Entity Framework Core features such as Include(), Where(), ToListAsync(), and database queries.
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Services;

// Static initializer service: prepares required roles and the default admin account when the app starts.
public static class DbInitializer
{
    // Service entry point used by Program.cs or controllers to prepare demo/runtime data.
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        string[] roles = ["Admin", "Organizer", "Volunteer"];

        foreach (var role in roles)
        {
<<<<<<< HEAD
            // Checks whether the required Identity role already exists before creating or assigning it.
=======
            // Check whether the required role already exists before creating it.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
            if (!await roleManager.RoleExistsAsync(role))
            {
                // Creates a missing Identity role so role-based authorization can work correctly.
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "admin@gmail.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        // Handles missing data safely before continuing with the requested operation.
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            // Create the default/demo Identity user account.
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

        // Retrieves a single matching database record asynchronously; returns null when not found.
        var adminProfile = await context.UserProfiles.FirstOrDefaultAsync(x => x.UserId == adminUser.Id);
        // Handles missing data safely before continuing with the requested operation.
        if (adminProfile == null)
        {
            // Creates an OCVMS profile record connected to an Identity user account.
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
