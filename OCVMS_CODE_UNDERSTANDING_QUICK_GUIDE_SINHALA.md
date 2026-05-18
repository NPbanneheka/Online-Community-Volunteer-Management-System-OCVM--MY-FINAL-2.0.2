# OCVMS Code Understanding Quick Guide

මෙම file එක ඔයාට code එක ඉක්මනට තේරුම් ගන්න හදාපු guide එකක්.

## Code එක explain කරන formula එක

Code block එකක් දැක්කම මේ 5 points වලින් explain කරන්න:

1. **මෙතන කරන්නේ මොකද්ද?**  
   Request එක handle කරනවද, data save කරනවද, validation කරනවද, permission check කරනවද කියලා කියන්න.

2. **භාවිතා කරලා තියෙන technology එක මොකද්ද?**  
   ASP.NET Core MVC, Entity Framework Core, Identity, LINQ, Razor View, Bootstrap වගේ terms identify කරන්න.

3. **Variables මොනවද?**  
   උදාහරණ: `user`, `profile`, `currentProfile`, `eventsQuery`, `model`, `_context`, `_userManager`.

4. **ඒ variables connect වෙන්නේ කොහෙටද?**  
   Database table එකකටද, model එකකටද, view එකකටද, Identity user එකකටද කියලා බලන්න.

5. **Security/validation තියෙනවද?**  
   `[Authorize]`, `ModelState.IsValid`, `Forbid()`, ownership checks, role checks වගේ දේවල් mention කරන්න.

## Most important files

- `Program.cs` - application startup, database, Identity, routing.
- `ApplicationDbContext.cs` - database tables and relationships.
- `AccountController.cs` - register, login, logout, password change.
- `EventsController.cs` - event create/edit/delete/join/rating.
- `ProfileController.cs` - profile edit, admin user management, verify/ban/delete.
- `CommunityController.cs` - community posts/comments.
- `HelpRequestsController.cs` - support/help request workflow.
- `NotificationsController.cs` - user notifications.
- `Models` folder - database table structures.
- `ViewModels` folder - form/page data structures.
- `Views` folder - UI pages.

## Strong technical words to use

- MVC architecture
- Role-based authorization
- ASP.NET Core Identity
- Password hashing
- Entity Framework Core
- LINQ query
- DbContext
- DbSet
- Foreign key relationship
- Ownership validation
- Server-side validation
- Razor View
- Dependency Injection

## Example explanation

`_context.VolunteerEvents.Include(e => e.OrganizerProfile)`

මේ code එකෙන් VolunteerEvents table එකෙන් event data load කරනවා. `Include` කියන්නේ EF Core feature එකක්. ඒක related `OrganizerProfile` data එකත් එකම query flow එකට load කරන්න use කරනවා. මේ data එක Events view එකේ organizer details display කරන්න connect වෙනවා.
