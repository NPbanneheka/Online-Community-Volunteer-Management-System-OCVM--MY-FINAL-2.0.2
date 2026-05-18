<<<<<<< HEAD
// Program.cs
// This application startup file that configures services, middleware, routing, Identity, and the database connection.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core Identity services for users, roles, passwords, sign-in sessions, and lockout/ban behavior.
=======
// Application startup and request pipeline configuration.
// Technology map:
// - ASP.NET Core MVC: controllers, views, routing, static files.
// - Entity Framework Core + SQL Server: database connection through ApplicationDbContext.
// - ASP.NET Core Identity: login, roles, password hashing, authentication cookies.
// - Dependency Injection: services are registered here and injected into controllers/services.
// Connected files: appsettings.json, ApplicationDbContext.cs, DbInitializer.cs, Controllers, Views.

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
// Entity Framework Core features such as Include(), Where(), ToListAsync(), and database queries.
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Services;
using System.Globalization;

<<<<<<< HEAD
// Creates the application builder and loads configuration such as appsettings.json.
=======
// Create the web application builder and load configuration/services.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
var builder = WebApplication.CreateBuilder(args);

// Read the SQL Server connection string from appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

<<<<<<< HEAD
// Enables MVC controllers and Razor views.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Registers ApplicationDbContext so EF Core can connect to SQL Server using dependency injection.
=======
// Enable MVC controllers and Razor views.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register Entity Framework Core with SQL Server.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    // Configures SQL Server as the database provider for Entity Framework Core.
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

<<<<<<< HEAD
// Registers ASP.NET Core Identity with user and role support.
=======
// Register ASP.NET Identity for users, roles, login, and password handling.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Student/demo friendly password policy. The RegisterViewModel still requires at least 6 characters.
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure login page, access denied page, cookie lifetime, and session behavior.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = false;
    options.Cookie.IsEssential = true;
});

<<<<<<< HEAD
// Builds the configured web application instance.
=======
// Build the configured application pipeline.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
// Allows the browser to load CSS, JavaScript, images, and other files from wwwroot.
app.UseStaticFiles();

var defaultCulture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
});

<<<<<<< HEAD
// Enables endpoint routing so URLs can map to MVC controller actions.
app.UseRouting();
// Reads authentication cookies and identifies the logged-in user.
app.UseAuthentication();
// Applies role and permission checks after authentication.
=======
// Routing decides which controller/action handles each URL.
app.UseRouting();
// Authentication checks who the current user is.
app.UseAuthentication();
// Authorization checks what the current user is allowed to do.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
app.UseAuthorization();

// Startup scope is used to apply runtime DB fixes and seed default roles/admin.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await EnsureRuntimeSchemaAsync(services);
        // Seeds required roles and the default admin account during application startup.
        await DbInitializer.SeedRolesAndAdminAsync(services);
        // Seeds demonstration records used to show the system with sample data.
        //await TestDataSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles and the default admin account.");
    }
}

<<<<<<< HEAD
// Defines the default MVC route pattern: Controller/Action/Id.
=======
// Default route: /Controller/Action/Id.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
// Starts the web server and keeps the application running.
app.Run();

<<<<<<< HEAD
=======
// Runtime safety method: adds missing DB columns when an older restored database is used.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
static async Task EnsureRuntimeSchemaAsync(IServiceProvider services)
{
    var context = services.GetRequiredService<ApplicationDbContext>();

    // These idempotent checks allow the updated project to run on the restored group database
    // even if the latest registration-date columns were not present in the original backup.
    // The ALTER statements are executed separately to avoid SQL Server parsing errors when columns do not exist yet.
    await context.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH('dbo.VolunteerEvents', 'RegistrationOpenDate') IS NULL
    EXEC('ALTER TABLE [dbo].[VolunteerEvents] ADD [RegistrationOpenDate] datetime2 NOT NULL CONSTRAINT [DF_VolunteerEvents_RegistrationOpenDate] DEFAULT (SYSUTCDATETIME()) WITH VALUES');
");

    await context.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH('dbo.VolunteerEvents', 'RegistrationClosingDate') IS NULL
    EXEC('ALTER TABLE [dbo].[VolunteerEvents] ADD [RegistrationClosingDate] datetime2 NULL');
");

    await context.Database.ExecuteSqlRawAsync(@"
UPDATE [dbo].[VolunteerEvents]
SET [RegistrationClosingDate] = [EventDate]
WHERE [RegistrationClosingDate] IS NULL;
");
}
