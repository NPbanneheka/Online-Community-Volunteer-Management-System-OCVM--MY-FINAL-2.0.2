BEGIN TRANSACTION;

DECLARE @SeedUsers TABLE (Id nvarchar(450));
DECLARE @SeedProfiles TABLE (Id int);
DECLARE @SeedEvents TABLE (Id int);

INSERT INTO @SeedUsers (Id)
SELECT Id
FROM AspNetUsers
WHERE Email LIKE 'org%.organizer%@ocvms.local'
   OR Email LIKE 'volunteer%@ocvms.local';

INSERT INTO @SeedProfiles (Id)
SELECT Id
FROM UserProfiles
WHERE UserId IN (SELECT Id FROM @SeedUsers);

INSERT INTO @SeedEvents (Id)
SELECT Id
FROM VolunteerEvents
WHERE OrganizerProfileId IN (SELECT Id FROM @SeedProfiles);

DELETE FROM UserRatings
WHERE EventId IN (SELECT Id FROM @SeedEvents)
   OR FromUserId IN (SELECT Id FROM @SeedUsers)
   OR ToUserId IN (SELECT Id FROM @SeedUsers);

DELETE FROM EventRegistrations
WHERE VolunteerEventId IN (SELECT Id FROM @SeedEvents)
   OR UserId IN (SELECT Id FROM @SeedUsers);

DELETE FROM Notifications
WHERE UserProfileId IN (SELECT Id FROM @SeedProfiles);

DELETE FROM PostComments
WHERE UserProfileId IN (SELECT Id FROM @SeedProfiles)
   OR CommunityPostId IN (
        SELECT Id FROM CommunityPosts
        WHERE UserProfileId IN (SELECT Id FROM @SeedProfiles)
   );

DELETE FROM CommunityPosts
WHERE UserProfileId IN (SELECT Id FROM @SeedProfiles);

DELETE FROM HelpRequests
WHERE UserProfileId IN (SELECT Id FROM @SeedProfiles);

DELETE FROM VolunteerEvents
WHERE Id IN (SELECT Id FROM @SeedEvents);

DELETE FROM UserProfiles
WHERE Id IN (SELECT Id FROM @SeedProfiles);

DELETE FROM AspNetUserRoles
WHERE UserId IN (SELECT Id FROM @SeedUsers);

DELETE FROM AspNetUserClaims
WHERE UserId IN (SELECT Id FROM @SeedUsers);

DELETE FROM AspNetUserLogins
WHERE UserId IN (SELECT Id FROM @SeedUsers);

DELETE FROM AspNetUserTokens
WHERE UserId IN (SELECT Id FROM @SeedUsers);

DELETE FROM AspNetUsers
WHERE Id IN (SELECT Id FROM @SeedUsers);

COMMIT TRANSACTION;