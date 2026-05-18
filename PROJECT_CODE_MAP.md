# OCVMS Project Code Map

This document is a technical reading guide for the commented source code. It explains what each important file is responsible for, the technologies used, the important variables/services, and the connected project parts.

## Main MVC Flow

Browser request → Controller action → Model/ViewModel → ApplicationDbContext / SQL Server → Razor View → Browser response.

## Most Important Technologies

- **ASP.NET Core MVC:** separates the application into Controllers, Models, and Views.
- **Entity Framework Core:** connects C# classes to SQL Server tables and handles database queries.
- **ASP.NET Core Identity:** handles users, roles, password hashing, login, logout, and account lockout.
- **Razor Views:** generate dynamic HTML pages using C# and HTML.
- **Dependency Injection:** services such as `ApplicationDbContext` and `UserManager` are injected into controllers.
- **LINQ:** used to filter, sort, join, and load data from EF Core queries.

## File-by-file Code Reading Notes

## Program.cs

**Purpose:** Application startup and request pipeline configuration.

**Main technologies:** ASP.NET Core startup, Dependency Injection, EF Core, Identity, Middleware

**Important variables/services:** builder, connectionString, app, scope, services

**Connected parts:** appsettings.json, ApplicationDbContext, DbInitializer, Controllers

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Controllers/AccountController.cs

**Purpose:** Handles account access: registration, login, logout, password change, and access denied.

**Main technologies:** ASP.NET Core MVC, Identity, EF Core

**Important variables/services:** _userManager, _signInManager, _roleManager, _context

**Connected parts:** Identity users/roles, UserProfiles table, Account views

**Key methods/properties:** Register, Register, Login, Login, ChangePassword, ChangePassword, Logout, AccessDenied

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Controllers/CommunityController.cs

**Purpose:** Handles the community feed, posts, comments, and post-related notifications.

**Main technologies:** ASP.NET Core MVC, EF Core LINQ, Identity, Authorization

**Important variables/services:** _context, _userManager, postsQuery, currentProfile, isAdmin

**Connected parts:** CommunityPosts, PostComments, Notifications, UserProfiles, Community views

**Key methods/properties:** Feed, CreatePost, AddComment, EditPost, EditPost, DeletePost, DeleteComment, GetCurrentProfileAsync, GetPrimaryProfileForUserAsync, CanManagePostAsync, CanManageCommentAsync

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Controllers/DashboardController.cs

**Purpose:** Builds the role-aware dashboard summary shown after login.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Index, GetPrimaryProfileForUserAsync

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Controllers/EventsController.cs

**Purpose:** Main event workflow controller: listing, creating, editing, deleting, joining, cancelling, and rating events.

**Main technologies:** ASP.NET Core MVC, EF Core LINQ, Identity, File Upload, Authorization

**Important variables/services:** _context, _userManager, _environment, eventsQuery, currentProfile

**Connected parts:** VolunteerEvents, EventRegistrations, UserProfiles, UserRatings, Notifications, Events views

**Key methods/properties:** Index, MyEvents, MyCreatedEvents, Register, Unregister, Details, Create, Create, Edit, Edit, Delete, JoinedVolunteers

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Controllers/HelpRequestsController.cs

**Purpose:** Handles support/help request creation, tracking, editing, resolving, and deletion.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Index, Create, Create, Details, Edit, Edit, Delete, UpdateStatus, GetCurrentProfileAsync, GetPrimaryProfileForUserAsync, CanViewHelpRequestAsync, CanManageHelpRequestAsync

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Controllers/HomeController.cs

**Purpose:** Handles public home pages such as landing, about, privacy, and error handling.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Index, Privacy, About, RateApplication, Error

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Controllers/NotificationsController.cs

**Purpose:** Handles user notifications such as viewing, marking as read, and deleting notifications.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Index, MarkAsRead, MarkAllAsRead, GetPrimaryProfileForUserAsync

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Controllers/ProfileController.cs

**Purpose:** Handles profiles and admin-side user management: view/edit profile, verify, ban, unban, and delete users.

**Main technologies:** ASP.NET Core MVC, Identity, EF Core, File Upload, Admin Authorization

**Important variables/services:** _context, _userManager, _environment, profile, user, model

**Connected parts:** AspNetUsers, UserProfiles, Events, Posts, Comments, Ratings, Profile views

**Key methods/properties:** MyProfile, Edit, Edit, ViewProfile, Index, ManageUsers, VerifyUser, UnverifyUser, TempBanUser, UnbanUser, SetVerificationStatusAsync, SetTemporaryBanStatusAsync

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Data/ApplicationDbContext.cs

**Purpose:** Central Entity Framework Core database context.

**Main technologies:** Entity Framework Core, ASP.NET Identity

**Important variables/services:** DbSet<T>, modelBuilder

**Connected parts:** All SQL Server tables and relationships

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/BaseEntity.cs

**Purpose:** Data model for BaseEntity.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/CommunityPost.cs

**Purpose:** Data model for CommunityPost.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/ErrorViewModel.cs

**Purpose:** Data model for ErrorViewModel.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/EventRegistration.cs

**Purpose:** Data model for EventRegistration.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/HelpRequest.cs

**Purpose:** Data model for HelpRequest.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/Notification.cs

**Purpose:** Data model for Notification.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/PostComment.cs

**Purpose:** Data model for PostComment.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/UserProfile.cs

**Purpose:** Data model for UserProfile.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/UserRating.cs

**Purpose:** Data model for UserRating.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Models/VolunteerEvent.cs

**Purpose:** Data model for VolunteerEvent.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Services/DbInitializer.cs

**Purpose:** Seeds default roles and the initial administrator account.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## Services/TestDataSeeder.cs

**Purpose:** Optional development/test data seeder.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## ViewModels/ChangePasswordViewModel.cs

**Purpose:** View model for ChangePasswordViewModel.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## ViewModels/DashboardViewModel.cs

**Purpose:** View model for DashboardViewModel.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## ViewModels/LoginViewModel.cs

**Purpose:** View model for LoginViewModel.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## ViewModels/ProfileEditViewModel.cs

**Purpose:** View model for ProfileEditViewModel.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.

## ViewModels/RegisterViewModel.cs

**Purpose:** View model for RegisterViewModel.

**Main technologies:** ASP.NET Core / C# / EF Core where applicable

**Important variables/services:** class properties and local variables

**Connected parts:** related controllers, models, views, and database tables

**Key methods/properties:** Properties / configuration only

**How to explain in presentation/review:** This file is part of the MVC flow. It receives or stores data, connects with related models/database tables, and supports the user-facing pages through controllers and Razor views.


## Important Explanation Pattern

When asked about a code block, explain it using this structure:

1. **Purpose:** what the block does.
2. **Technology:** MVC / EF Core / Identity / Razor / LINQ / File Upload.
3. **Variables:** what values are stored.
4. **Connections:** which model, database table, view, or controller it connects with.
5. **Security:** authentication, authorization, validation, or ownership checking if used.

Example:

`var currentProfile = await GetCurrentProfileAsync();`

- Purpose: loads the application profile of the logged-in user.
- Technology: ASP.NET Identity identifies the user; EF Core loads the matching UserProfile.
- Variable: `currentProfile` stores the profile record.
- Connection: connected to `AspNetUsers`, `UserProfiles`, and controller permission checks.
