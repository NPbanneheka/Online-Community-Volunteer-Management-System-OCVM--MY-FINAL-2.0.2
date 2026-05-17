# OVMS / OCVMS Viva Code Reading Guide

This ZIP is a commented learning version of the project. The application logic was not intentionally changed; comments were added to help you understand and explain the code during viva.

## Best order to study the project

1. `Program.cs`  
   Learn how the app starts: services, SQL Server connection, Identity, authentication, authorization, routing, role/admin seeding.

2. `Data/ApplicationDbContext.cs`  
   Learn how EF Core maps C# classes to SQL Server tables and relationships.

3. `Models/`  
   Learn the database entities: `UserProfile`, `VolunteerEvent`, `EventRegistration`, `CommunityPost`, `HelpRequest`, `Notification`, `UserRating`.

4. `ViewModels/`  
   Learn how form data is validated before saving, especially login, register, profile edit, and change password.

5. `Controllers/`  
   Learn the system workflow. Controllers receive user requests, check roles/ownership, read/write database data, then return views.

6. `Views/`  
   Learn how Razor pages display controller data and send forms back to controller actions.

## Viva explanation pattern

When examiner asks about any function, explain in this order:

1. **Purpose**: what this part does.
2. **Input**: what data comes from user/form/URL.
3. **Validation**: what checks are done.
4. **Database action**: what table is read/inserted/updated/deleted.
5. **Security**: role check, login check, or ownership check.
6. **Output**: which view/redirect/message is returned.

## Very important files for viva

- `Program.cs` - startup and security pipeline.
- `Data/ApplicationDbContext.cs` - database connection and EF relationships.
- `Controllers/AccountController.cs` - login/register/password.
- `Controllers/EventsController.cs` - event create/edit/delete/register/cancel/rating.
- `Controllers/ProfileController.cs` - profile and admin user management.
- `Controllers/CommunityController.cs` - posts and comments.
- `Controllers/HelpRequestsController.cs` - support requests.
- `Controllers/DashboardController.cs` - role-based dashboard.

## Strong viva answer for role-based security

The system uses ASP.NET Identity roles: Admin, Organizer, and Volunteer. Controller actions use `[Authorize]` and `[Authorize(Roles = "...")]` to restrict access. For sensitive actions such as editing or deleting events, the controller also checks ownership/business rules before saving changes. This prevents normal users from managing records that do not belong to them.

## Strong viva answer for database

The project uses Entity Framework Core with SQL Server. `ApplicationDbContext` contains `DbSet` properties for the main entities. Controllers use LINQ queries such as `Where`, `Include`, `FirstOrDefaultAsync`, and `ToListAsync` to read data. Insert/update/delete changes are committed using `SaveChangesAsync()`.

## Strong viva answer for MVC

The project follows MVC architecture. Models represent data, Views display UI, and Controllers handle user requests and business logic. ViewModels are used for form validation and to avoid exposing unnecessary database fields directly to the UI.
