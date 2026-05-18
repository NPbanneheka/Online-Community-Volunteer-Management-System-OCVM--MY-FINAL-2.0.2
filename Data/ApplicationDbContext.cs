<<<<<<< HEAD
// Data/ApplicationDbContext.cs
// This Entity Framework Core data access file that maps C# models to SQL Server tables.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core Identity services for users, roles, passwords, sign-in sessions, and lockout/ban behavior.
=======
// Central Entity Framework Core database context.
// Technology map:
// - IdentityDbContext: adds ASP.NET Identity tables such as AspNetUsers and AspNetRoles.
// - DbSet<T>: maps C# model classes to SQL Server database tables.
// - Fluent API: configures relationships, indexes, and delete behavior.
// Connected files: Program.cs registers this context; Controllers query it; Models define table structures.

>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// Entity Framework Core features such as Include(), Where(), ToListAsync(), and database queries.
using Microsoft.EntityFrameworkCore;
using OCVMS.Models;

// Namespace groups related OCVMS classes so they can be referenced cleanly across the project.
namespace OCVMS.Data;

// DbContext class: central EF Core gateway between C# models and SQL Server tables.
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    // Constructor receives DB options configured in Program.cs.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

<<<<<<< HEAD
    // DbSet exposes a model as a queryable/updatable table through Entity Framework Core.
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    // DbSet exposes a model as a queryable/updatable table through Entity Framework Core.
    public DbSet<VolunteerEvent> VolunteerEvents => Set<VolunteerEvent>();
    // DbSet exposes a model as a queryable/updatable table through Entity Framework Core.
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    // DbSet exposes a model as a queryable/updatable table through Entity Framework Core.
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();
    // DbSet exposes a model as a queryable/updatable table through Entity Framework Core.
    public DbSet<PostComment> PostComments => Set<PostComment>();
    // DbSet exposes a model as a queryable/updatable table through Entity Framework Core.
    public DbSet<UserRating> UserRatings => Set<UserRating>();
    // DbSet exposes a model as a queryable/updatable table through Entity Framework Core.
    public DbSet<HelpRequest> HelpRequests => Set<HelpRequest>();
    // DbSet exposes a model as a queryable/updatable table through Entity Framework Core.
=======
    // DbSet maps the UserProfile model to the UserProfiles table/query.
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    // DbSet maps the VolunteerEvent model to the VolunteerEvents table/query.
    public DbSet<VolunteerEvent> VolunteerEvents => Set<VolunteerEvent>();

    // DbSet maps the EventRegistration model to the EventRegistrations table/query.
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    // DbSet maps the CommunityPost model to the CommunityPosts table/query.
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();

    // DbSet maps the PostComment model to the PostComments table/query.
    public DbSet<PostComment> PostComments => Set<PostComment>();

    // DbSet maps the UserRating model to the UserRatings table/query.
    public DbSet<UserRating> UserRatings => Set<UserRating>();

    // DbSet maps the HelpRequest model to the HelpRequests table/query.
    public DbSet<HelpRequest> HelpRequests => Set<HelpRequest>();

    // DbSet maps the Notification model to the Notifications table/query.
>>>>>>> 36052103534a4f80c4dd0c1d9df8322a619f0675
    public DbSet<Notification> Notifications => Set<Notification>();

    // Configures EF Core relationships, unique indexes, and delete behavior.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        modelBuilder.Entity<EventRegistration>()
            .HasIndex(x => new { x.VolunteerEventId, x.UserId })
            .IsUnique();

        modelBuilder.Entity<UserRating>()
            .HasIndex(x => new { x.EventId, x.FromUserId, x.ToUserId })
            .IsUnique();

        modelBuilder.Entity<VolunteerEvent>()
            .HasOne(ve => ve.OrganizerProfile)
            .WithMany(u => u.OrganizedEvents)
            .HasForeignKey(ve => ve.OrganizerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventRegistration>()
            .HasOne(er => er.VolunteerEvent)
            .WithMany(ve => ve.Registrations)
            .HasForeignKey(er => er.VolunteerEventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventRegistration>()
            .HasOne(er => er.User)
            .WithMany()
            .HasForeignKey(er => er.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommunityPost>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PostComment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.PostComments)
            .HasForeignKey(c => c.CommunityPostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostComment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HelpRequest>()
            .HasOne(h => h.SubmittedBy)
            .WithMany(u => u.HelpRequests)
            .HasForeignKey(h => h.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
