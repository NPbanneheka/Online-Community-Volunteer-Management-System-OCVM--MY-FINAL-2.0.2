# OCVMS - Online Community Volunteer Management System

OCVMS is an ASP.NET Core MVC web application developed as a community volunteer management platform. It supports volunteer event management, event registration, profile management, community engagement, support requests, notifications, and dashboard summaries.

## Technology Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server / SQL Server LocalDB
- ASP.NET Core Identity
- Bootstrap 5

## Main User Roles

- **Admin**: can manage all users, events, community posts, support requests, verification, and system activity.
- **Organizer**: can create and manage only their own volunteer events, and participate in community help discussions.
- **Volunteer**: can browse events, register/unregister, maintain a profile, post in the community hub, ask for community help and submit support requests.

## Implemented Features

1. User registration, login, logout
2. Role-based access control
3. Volunteer event listing and search
4. Event creation, editing, deleting, and details page
5. Volunteer event registration and unregistering
6. Capacity checking to prevent over-registration
7. User profile management with profile photo upload
8. Community posts and comments
9. Support request submission and admin status tracking
10. User notifications
11. Dashboard with event, registration, post, and rating summary
12. Default admin seeding
13. Admin user verification and Organizer/Volunteer account deletion
14. Change password feature
15. Strict ownership authorization for events, community posts, comments, and support requests

## Default Admin Account

- Email: `admin@gmail.com`
- Password: `123456`

- Email: `admin@ocvms.local`
- Password: `Admin123`

## Database Setup

The current connection string is in `appsettings.json`:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=OCVMSDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

If your PC uses LocalDB instead of the default SQL Server instance, replace it with:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=OCVMSDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

## Run Steps

Open the project folder in Visual Studio or terminal and run:

```bash
dotnet restore .\OCVMS.csproj
dotnet ef database update --project .\OCVMS.csproj --startup-project .\OCVMS.csproj
dotnet run --project ".\OCVMS.csproj"
```

Then open the shown localhost URL in the browser.

## Notes for Academic Submission

This final version keeps the main SRS scope aligned with the implemented application:

- Volunteer and organizer management are implemented through Identity roles and user profiles.
- Community engagement is implemented through posts, comments, and help-request support.
- Notifications are implemented for important actions such as event creation, event registration, support requests, and comments.
- Extended features can still be improved further in future work, such as advanced reports, email/SMS notifications, and a complete rating workflow.


## Final Polished Security Rules

- Admin can manage all records.
- Organizer can edit/delete only events created by their own profile.
- Volunteer and Organizer can edit/delete only their own community posts and support requests.
- Community post owners can remove comments on their own posts.
- Controller checks protect direct URL access, not only UI buttons.
- Verified/latest profile lookup is used to avoid older duplicate-profile rows showing Pending incorrectly.

## Final Testing Checklist

Before final submission, test these flows:

1. Login using `admin@ocvms.local` / `Admin123`.
2. Register one Organizer and one Volunteer.
3. Admin verifies the Organizer from Profile > User Management.
4. Organizer profile shows `Verified` instead of `Pending Verification`.
5. Organizer creates an event.
6. Volunteer registers for the event and sees it under My Events.
7. Organizer can view Joined Volunteers and send notification.
8. Another Organizer cannot edit/delete the first Organizer's event.
9. Volunteer cannot edit/delete any event.
10. Users can change password from Profile or navbar.
11. Admin can delete a Volunteer/Organizer account, but cannot delete Admin accounts.
12. Community post/comment ownership buttons appear only for allowed users.
13. Support request status update is Admin-only.
