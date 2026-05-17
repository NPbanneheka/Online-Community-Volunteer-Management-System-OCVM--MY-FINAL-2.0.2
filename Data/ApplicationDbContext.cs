// ================================================================
// VIVA COMMENTED VERSION - Data/ApplicationDbContext.cs
// Purpose: EF Core database context: connects C# models with SQL Server tables and configures relationships.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OCVMS.Models;

namespace OCVMS.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    // Constructor receives DB options configured in Program.cs.
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

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
