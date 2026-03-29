# OCVMS - Online Community Volunteer Management System

This is a modern ASP.NET Core MVC starter project for a community volunteer management platform.

## Features
- User registration and login
- Roles: Admin, Organizer, Volunteer
- Modern dashboard
- Personal profile pages
- Event listing and event registration
- Organizer event creation
- Community feed and comments
- Ratings model ready for trust/reputation features

## Tech Stack
- ASP.NET Core MVC (.NET 8)
- C#
- Entity Framework Core
- SQL Server / LocalDB
- Bootstrap 5 + custom CSS

## How to Run
1. Install .NET 8 SDK
2. Open the project folder
3. Restore packages:
   ```bash
   dotnet restore
   ```
4. Create migration:
   ```bash
   dotnet ef migrations add InitialCreate
   ```
5. Update database:
   ```bash
   dotnet ef database update
   ```
6. Run the app:
   ```bash
   dotnet run
   ```

## Default admin account
- Email: `admin@ocvms.local`
- Password: `Admin123`

## Notes
- This package was generated in an environment without the .NET SDK, so the source structure was prepared carefully but not compiled inside the container.
- If you prefer, you can switch the connection string to SQL Server Express or full SQL Server.
