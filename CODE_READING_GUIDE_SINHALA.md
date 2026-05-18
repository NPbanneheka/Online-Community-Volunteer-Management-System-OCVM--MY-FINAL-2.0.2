# OCVMS Code Reading Guide - Sinhala Notes

මෙම package එකේ comments add කරලා තියෙන්නේ project code එක professional විදියට තේරුම් ගන්න. Comments කිසිම business logic එකක් වෙනස් කරන්නේ නැහැ.

## Code එක බලන පිළිවෙල

1. `Program.cs` - app startup, services, Identity, database connection, middleware, routing.
2. `Data/ApplicationDbContext.cs` - EF Core DbContext සහ tables/DbSet mappings.
3. `Models/` - database entities: UserProfile, VolunteerEvent, EventRegistration, CommunityPost, Notification, HelpRequest.
4. `ViewModels/` - forms/dashboard වලට යන data objects සහ validation rules.
5. `Controllers/` - browser request handle කරන logic: validation, authorization, EF queries, redirects/views.
6. `Views/` - Razor UI pages. Controller එකෙන් එන model/viewbag data browser එකේ display කරන තැන.

## Viva එකට මතක තියාගන්න

- ASP.NET Core MVC: Model, View, Controller separation.
- Entity Framework Core: C# code වලින් SQL Server database access කිරීම.
- ASP.NET Core Identity: login, register, password hashing, roles, ban/lockout.
- Role-based authorization: Admin, Organizer, Volunteer permissions.
- Ownership checks: users can manage their own records; Admin can manage all.

## Comment style ගැන

Code එක තුළ ඇති comments developer comments විදියට දාලා තියෙනවා. ඒවා project understanding, technology usage, variable/data connection තේරුම් ගන්න උපකාරී වෙනවා.
