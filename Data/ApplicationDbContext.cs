using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OCVMS.Models;

namespace OCVMS.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<VolunteerEvent> VolunteerEvents => Set<VolunteerEvent>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();
    public DbSet<PostComment> PostComments => Set<PostComment>();
    public DbSet<UserRating> UserRatings => Set<UserRating>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserProfile>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        builder.Entity<EventRegistration>()
            .HasIndex(x => new { x.VolunteerEventId, x.UserId })
            .IsUnique();

        builder.Entity<UserRating>()
            .HasIndex(x => new { x.EventId, x.FromUserId, x.ToUserId })
            .IsUnique();
    }
}
