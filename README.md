# OCVMS - Online Community Volunteer Management System

OCVMS is an ASP.NET Core MVC web application developed as a community volunteer management platform. It supports volunteer event management, event registration, profile management, community engagement, help requests, notifications, and dashboard summaries.

## Technology Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server / SQL Server LocalDB
- ASP.NET Core Identity
- Bootstrap 5

## Main User Roles

- **Admin**: can manage events and review system activity.
- **Organizer**: can create/manage volunteer events and update help request statuses.
- **Volunteer**: can browse events, register/unregister, maintain a profile, post in the community hub, and submit help requests.

## Implemented Features

1. User registration, login, logout
2. Role-based access control
3. Volunteer event listing and search
4. Event creation, editing, deleting, and details page
5. Volunteer event registration and unregistering
6. Capacity checking to prevent over-registration
7. User profile management with profile photo upload
8. Community posts and comments
9. Help request submission and status tracking
10. User notifications
11. Dashboard with event, registration, post, and rating summary
12. Default admin seeding

## Default Admin Account

- Email: `admin@ocvms.local`
- Password: `Admin123`

## Database Setup

The current connection string is in `appsettings.json`:

```json
"DefaultConnection": "Server=.;Database=OCVMSDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

If your PC uses LocalDB instead of the default SQL Server instance, replace it with:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=OCVMSDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

## Run Steps

Open the project folder in Visual Studio or terminal and run:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Then open the shown localhost URL in the browser.

## Notes for Academic Submission

This final version keeps the main SRS scope aligned with the implemented application:

- Volunteer and organizer management are implemented through Identity roles and user profiles.
- Community engagement is implemented through posts, comments, and help-request support.
- Notifications are implemented for important actions such as event creation, event registration, help requests, and comments.
- Extended features can still be improved further in future work, such as advanced reports, email/SMS notifications, and a complete rating workflow.
