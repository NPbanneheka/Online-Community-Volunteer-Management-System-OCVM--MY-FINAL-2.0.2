// Data/ApplicationDbContext.cs
// This Entity Framework Core data access file that maps C# models to SQL Server tables.
// Comments explain purpose, connected technologies, variables, and data flow without changing behavior.
// ASP.NET Core Identity services for users, roles, passwords, sign-in sessions, and lockout/ban behavior.
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
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

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
    public DbSet<Notification> Notifications => Set<Notification>();

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
