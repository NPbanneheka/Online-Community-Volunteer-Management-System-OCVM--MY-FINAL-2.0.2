// ================================================================
// VIVA COMMENTED VERSION - Services/TestDataSeeder.cs
// Purpose: Optional demo-data seeder used to populate sample users, events, posts, and notifications.
// Note: Comments were added for learning/viva explanation. Business logic is unchanged.
// ================================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OCVMS.Data;
using OCVMS.Models;

namespace OCVMS.Services;

public static class TestDataSeeder
{
    private const string TestPassword = "123";
    // Service entry point used by Program.cs or controllers to prepare demo/runtime data.
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<IdentityUser>>();

        await EnsureRolesAsync(roleManager);

        var organizations = new[]
        {
            "Green Hope Foundation",
            "LifeCare Volunteers",
            "Bright Future Sri Lanka",
            "Helping Hands Network",
            "Unity Community Services",
            "EcoCare Volunteers",
            "HopeBridge Organization",
            "Kind Hearts Foundation",
            "SafeHands Community",
            "Rise Together Lanka"
        };

        var organizerProfiles = new List<UserProfile>();

        for (var orgIndex = 0; orgIndex < organizations.Length; orgIndex++)
        {
            var organizationName = organizations[orgIndex];
            var orgCode = (orgIndex + 1).ToString("00");

            for (var organizerIndex = 1; organizerIndex <= 5; organizerIndex++)
            {
                var email = $"org{orgCode}.organizer{organizerIndex:00}@ocvms.local";
                var fullName = $"{organizationName} Organizer {organizerIndex:00}";

                var profile = await EnsureUserWithProfileAsync(
                    context,
                    userManager,
                    passwordHasher,
                    email,
                    fullName,
                    "Organizer",
                    organizationName,
                    contactNumber: $"077{orgIndex + 1:00}{organizerIndex:00}{organizerIndex:000}",
                    skills: "Event planning, volunteer coordination, community communication",
                    availability: "Weekdays evenings and weekends",
                    bio: $"Organizer representing {organizationName} for community volunteering activities.");

                profile.IsVerified = true;
                organizerProfiles.Add(profile);
            }
        }

        var volunteerProfiles = new List<UserProfile>();

        for (var volunteerIndex = 1; volunteerIndex <= 30; volunteerIndex++)
        {
            var email = $"volunteer{volunteerIndex:00}@ocvms.local";
            var fullName = $"Volunteer User {volunteerIndex:00}";

            var profile = await EnsureUserWithProfileAsync(
                context,
                userManager,
                passwordHasher,
                email,
                fullName,
                "Volunteer",
                organizationName: "Independent Volunteer",
                contactNumber: $"076{volunteerIndex:00}{volunteerIndex:0000}",
                skills: GetVolunteerSkills(volunteerIndex),
                availability: GetVolunteerAvailability(volunteerIndex),
                bio: "Community volunteer interested in supporting social service activities and public events.");

            profile.IsVerified = volunteerIndex % 3 != 0;
            volunteerProfiles.Add(profile);
        }

        await context.SaveChangesAsync();

        organizerProfiles = await context.UserProfiles
            .Where(p => p.RoleName == "Organizer" && p.OrganizationName != null)
            .OrderBy(p => p.OrganizationName)
            .ThenBy(p => p.FullName)
            .ToListAsync();

        await SeedEventsAsync(context, organizerProfiles);
        await SeedRegistrationsAndRatingsAsync(context, userManager, volunteerProfiles);
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "Organizer", "Volunteer" })
        {
            if (!// Check whether the required role already exists before creating it.
            await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task<UserProfile> EnsureUserWithProfileAsync(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        IPasswordHasher<IdentityUser> passwordHasher,
        string email,
        string fullName,
        string roleName,
        string organizationName,
        string contactNumber,
        string skills,
        string availability,
        string bio)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var createResult = // Create the default/demo Identity user account.
            await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Could not create user {email}: " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        user.PasswordHash = passwordHasher.HashPassword(user, TestPassword);
        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            await userManager.AddToRoleAsync(user, roleName);
        }

        var profile = await context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = user.Id,
                FullName = fullName,
                RoleName = roleName,
                PublicEmail = email,
                ContactNumber = contactNumber,
                OrganizationName = organizationName,
                Skills = skills,
                Availability = availability,
                Bio = bio,
                CreatedAt = DateTime.UtcNow,
                IsVerified = roleName == "Organizer"
            };

            context.UserProfiles.Add(profile);
            await context.SaveChangesAsync();
        }
        else
        {
            profile.FullName = fullName;
            profile.RoleName = roleName;
            profile.PublicEmail = email;
            profile.ContactNumber = contactNumber;
            profile.OrganizationName = organizationName;
            profile.Skills = skills;
            profile.Availability = availability;
            profile.Bio = bio;
        }

        return profile;
    }

    private static async Task SeedEventsAsync(ApplicationDbContext context, List<UserProfile> organizerProfiles)
    {
        if (!organizerProfiles.Any())
        {
            return;
        }

        var eventTitles = new[]
        {
            "Community Blood Donation Campaign",
            "Beach Cleaning and Plastic Awareness Drive",
            "School Supplies Distribution Program",
            "Elderly Care Home Support Day",
            "Tree Planting and Green City Project",
            "Free Medical Screening Camp",
            "Disaster Relief Packing Volunteer Day",
            "Children's Reading and Learning Workshop",
            "Food Donation Drive for Low-Income Families",
            "Community Water Safety Awareness Session",
            "Village Roadside Cleaning Campaign",
            "Animal Shelter Support and Feeding Day",
            "Public Park Restoration Volunteer Project",
            "Youth Career Guidance and Mentoring Session",
            "Community Health Awareness Walk",
            "Donation Collection for Flood-Affected Families",
            "Local Temple and Community Hall Cleaning Day",
            "Children's Art and Creativity Workshop",
            "Senior Citizens Digital Literacy Support",
            "Community Garden Development Project",
            "Free Eye Checkup and Glasses Donation Camp",
            "Emergency Response Volunteer Training",
            "Mental Health Awareness Community Session",
            "Women Empowerment Skill Sharing Workshop",
            "Recycling Awareness and Waste Sorting Campaign",
            "Rural Library Book Donation Program",
            "Public Transport Safety Awareness Event",
            "Community Kitchen Meal Preparation Day",
            "Sports Day Support for Underprivileged Children",
            "Night School Support Volunteer Program",
            "Rainy Season Dengue Prevention Campaign",
            "Hospital Visitor Support Volunteer Session",
            "Youth Leadership Development Workshop",
            "Clean Drinking Water Distribution Program",
            "Community First Aid Awareness Program",
            "Local Farmers Support Market Day",
            "Orphanage Educational Activity Day",
            "City Clean-Up Morning Volunteer Project",
            "Road Safety Awareness for School Children",
            "Public Library Organizing Volunteer Day",
            "Community Cultural Event Support Program",
            "Emergency Generator Support Coordination Event",
            "Nutrition Awareness Session for Families",
            "Volunteer Orientation and Training Day",
            "Community Recycling Collection Weekend",
            "Charity Clothing Distribution Program",
            "School Environment Beautification Project",
            "Awareness Campaign for Blood Donation",
            "Community Support for Special Needs Children",
            "Green Village Sustainable Living Workshop"
        };

        var locations = new[]
        {
            "Colombo Community Hall",
            "Galle Face Green",
            "Kandy Municipal Hall",
            "Negombo Public Ground",
            "Kurunegala Town Hall",
            "Gampaha District Center",
            "Matara Beach Area",
            "Jaffna Community Center",
            "Ratnapura Youth Center",
            "Anuradhapura Public Library"
        };

        for (var i = 0; i < eventTitles.Length; i++)
        {
            var title = eventTitles[i];
            var exists = await context.VolunteerEvents.AnyAsync(e => e.Title == title);
            if (exists)
            {
                continue;
            }

            var organizer = organizerProfiles[i % organizerProfiles.Count];
            var eventDate = DateTime.Today.AddDays(7 + i);
            var registrationOpenDate = DateTime.Today.AddDays(-3 + (i % 3));
            var registrationClosingDate = eventDate.AddDays(-1);

            var volunteerEvent = new VolunteerEvent
            {
                Title = title,
                Description = GetEventDescription(title, organizer.OrganizationName ?? "Community Organization"),
                Location = locations[i % locations.Length],
                EventDate = eventDate,
                EventTime = TimeSpan.FromHours(8 + (i % 8)),
                RegistrationOpenDate = registrationOpenDate,
                RegistrationClosingDate = registrationClosingDate,
                Capacity = 25 + ((i * 7) % 75),
                Status = "Upcoming",
                OrganizerProfileId = organizer.Id,
                ImageUrl = null,
                CreatedAt = DateTime.UtcNow.AddDays(-(i % 10))
            };

            context.VolunteerEvents.Add(volunteerEvent);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRegistrationsAndRatingsAsync(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        List<UserProfile> volunteerProfiles)
    {
        var events = await context.VolunteerEvents
            .Include(e => e.OrganizerProfile)
            .OrderBy(e => e.EventDate)
            .Take(50)
            .ToListAsync();

        if (!events.Any() || !volunteerProfiles.Any())
        {
            return;
        }

        var volunteerUsers = new Dictionary<int, IdentityUser>();
        foreach (var volunteer in volunteerProfiles)
        {
            var user = await userManager.FindByEmailAsync(volunteer.PublicEmail ?? string.Empty);
            if (user != null)
            {
                volunteerUsers[volunteer.Id] = user;
            }
        }

        for (var eventIndex = 0; eventIndex < events.Count; eventIndex++)
        {
            var currentEvent = events[eventIndex];
            var registrationCount = Math.Min(8, Math.Max(3, currentEvent.Capacity / 10));

            for (var j = 0; j < registrationCount; j++)
            {
                var volunteer = volunteerProfiles[(eventIndex + j) % volunteerProfiles.Count];
                if (!volunteerUsers.TryGetValue(volunteer.Id, out var volunteerUser))
                {
                    continue;
                }

                var alreadyRegistered = await context.EventRegistrations.AnyAsync(r =>
                    r.VolunteerEventId == currentEvent.Id && r.UserId == volunteerUser.Id);

                if (!alreadyRegistered)
                {
                    context.EventRegistrations.Add(new EventRegistration
                    {
                        VolunteerEventId = currentEvent.Id,
                        UserId = volunteerUser.Id,
                        RegistrationDate = DateTime.Now.AddDays(-(j + 1)),
                        CreatedAt = DateTime.UtcNow.AddDays(-(j + 1))
                    });
                }

                if (currentEvent.OrganizerProfile?.UserId != null && eventIndex % 4 == 0)
                {
                    var alreadyRated = await context.UserRatings.AnyAsync(r =>
                        r.EventId == currentEvent.Id &&
                        r.FromUserId == volunteerUser.Id &&
                        r.ToUserId == currentEvent.OrganizerProfile.UserId);

                    if (!alreadyRated)
                    {
                        context.UserRatings.Add(new UserRating
                        {
                            EventId = currentEvent.Id,
                            FromUserId = volunteerUser.Id,
                            ToUserId = currentEvent.OrganizerProfile.UserId,
                            Score = 4 + ((eventIndex + j) % 2),
                            ReviewText = "Well organized and meaningful community event.",
                            CreatedAt = DateTime.UtcNow.AddDays(-(j + 1))
                        });
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static string GetVolunteerSkills(int index)
    {
        var skills = new[]
        {
            "First aid, event support, crowd coordination",
            "Teaching, mentoring, communication",
            "Photography, social media, event documentation",
            "Logistics, transport support, resource handling",
            "Cleaning campaigns, environmental awareness",
            "Food distribution, donation handling, teamwork"
        };

        return skills[index % skills.Length];
    }

    private static string GetVolunteerAvailability(int index)
    {
        var availability = new[]
        {
            "Weekends morning",
            "Weekends full day",
            "Weekday evenings",
            "Public holidays",
            "Flexible schedule"
        };

        return availability[index % availability.Length];
    }

    private static string GetEventDescription(string title, string organizationName)
    {
        return $"{organizationName} is organizing the {title.ToLower()} to support local communities through volunteer participation, resource coordination, and practical social service activities.";
    }
}
