using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));

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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = false;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var defaultCulture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await EnsureRuntimeSchemaAsync(services);
        await DbInitializer.SeedRolesAndAdminAsync(services);
        //await TestDataSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles and the default admin account.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();


static async Task EnsureRuntimeSchemaAsync(IServiceProvider services)
{
    var context = services.GetRequiredService<ApplicationDbContext>();

    // These idempotent checks allow the updated project to run on the restored group database
    // even if the latest registration-date columns were not present in the original backup.
    await context.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH('VolunteerEvents', 'RegistrationOpenDate') IS NULL
BEGIN
    ALTER TABLE [VolunteerEvents] ADD [RegistrationOpenDate] datetime2 NOT NULL CONSTRAINT [DF_VolunteerEvents_RegistrationOpenDate] DEFAULT (SYSUTCDATETIME());
END;

IF COL_LENGTH('VolunteerEvents', 'RegistrationClosingDate') IS NULL
BEGIN
    ALTER TABLE [VolunteerEvents] ADD [RegistrationClosingDate] datetime2 NULL;
END;

UPDATE [VolunteerEvents]
SET [RegistrationClosingDate] = [EventDate]
WHERE [RegistrationClosingDate] IS NULL;
");
}
