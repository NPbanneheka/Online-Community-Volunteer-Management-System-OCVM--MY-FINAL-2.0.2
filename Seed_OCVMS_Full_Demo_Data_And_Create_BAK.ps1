
# OCVMS Full Demo Data Seeder + Backup Creator
# Run this from the project root folder where OCVMS.csproj is located.

$ErrorActionPreference = "Stop"

$server = ".\SQLEXPRESS"
$database = "OCVMSDb"
$backupPath = "C:\Temp\OCVMS_FULL_DEMO_DATA.bak"

if (!(Test-Path ".\OCVMS.csproj")) {
    Write-Host "ERROR: Please run this script from the OCVMS project root folder where OCVMS.csproj exists." -ForegroundColor Red
    exit 1
}

New-Item -ItemType Directory -Force -Path "C:\Temp" | Out-Null
New-Item -ItemType Directory -Force -Path ".\wwwroot\uploads\profiles" | Out-Null
New-Item -ItemType Directory -Force -Path ".\wwwroot\uploads\events" | Out-Null

Write-Host "Copying demo profile and event images..." -ForegroundColor Cyan
Copy-Item ".\demo_assets\uploads\profiles\*" ".\wwwroot\uploads\profiles\" -Force
Copy-Item ".\demo_assets\uploads\events\*" ".\wwwroot\uploads\events\" -Force

$connectionString = "Server=$server;Database=$database;Trusted_Connection=True;TrustServerCertificate=True"

$sql = @'

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF COL_LENGTH('dbo.VolunteerEvents', 'EventTime') IS NULL
    ALTER TABLE [dbo].[VolunteerEvents] ADD [EventTime] time NOT NULL CONSTRAINT [DF_VolunteerEvents_EventTime] DEFAULT ('09:00:00') WITH VALUES;

IF COL_LENGTH('dbo.VolunteerEvents', 'RegistrationOpenDate') IS NULL
    ALTER TABLE [dbo].[VolunteerEvents] ADD [RegistrationOpenDate] datetime2 NOT NULL CONSTRAINT [DF_VolunteerEvents_RegistrationOpenDate] DEFAULT (SYSUTCDATETIME()) WITH VALUES;

IF COL_LENGTH('dbo.VolunteerEvents', 'RegistrationClosingDate') IS NULL
    ALTER TABLE [dbo].[VolunteerEvents] ADD [RegistrationClosingDate] datetime2 NULL;

IF COL_LENGTH('dbo.CommunityPosts', 'PostType') IS NULL
    ALTER TABLE [dbo].[CommunityPosts] ADD [PostType] nvarchar(max) NOT NULL CONSTRAINT [DF_CommunityPosts_PostType] DEFAULT (N'Share') WITH VALUES;

BEGIN TRANSACTION;

DELETE FROM [UserRatings];
DELETE FROM [PostComments];
DELETE FROM [EventRegistrations];
DELETE FROM [Notifications];
DELETE FROM [HelpRequests];
DELETE FROM [CommunityPosts];
DELETE FROM [VolunteerEvents];
DELETE FROM [UserProfiles];

DELETE FROM [AspNetUserClaims];
DELETE FROM [AspNetUserLogins];
DELETE FROM [AspNetUserTokens];
DELETE FROM [AspNetUserRoles];
DELETE FROM [AspNetUsers];
DELETE FROM [AspNetRoleClaims];
DELETE FROM [AspNetRoles];

INSERT INTO [AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp]) VALUES
(N'role-admin',N'Admin',N'ADMIN',N'ff89a8e2-f271-4478-a427-1f42c8abf8b8'),
(N'role-organizer',N'Organizer',N'ORGANIZER',N'668c1c03-4161-4647-99d2-8a90ddf973c0'),
(N'role-volunteer',N'Volunteer',N'VOLUNTEER',N'5a535a63-af51-422c-984d-dba7e9a2a3c5');

INSERT INTO [AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnd],[LockoutEnabled],[AccessFailedCount]) VALUES
(N'user-admin',N'admin@ocvms.local',N'ADMIN@OCVMS.LOCAL',N'admin@ocvms.local',N'ADMIN@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEFm0Z1u4KYbmBy5drBf9ocTBc3Dt8zYgOxbOq3YuF/UOMLLI2Ni5xAKDOmdREq/6Ug==',N'e576e307-bfe1-4215-962f-1311b233af23',N'b4d97733-9e37-4bad-b78c-ef93ea337dce',NULL,1,0,NULL,1,0),
(N'user-org-green-01',N'nishan.green@ocvms.local',N'NISHAN.GREEN@OCVMS.LOCAL',N'nishan.green@ocvms.local',N'NISHAN.GREEN@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'de7cea2e-bbdd-4e12-ac1a-4bc71f1b3c0c',N'33a06010-732f-4000-a259-8ddfe03bf54e',NULL,1,0,NULL,1,0),
(N'user-org-green-02',N'akila.green@ocvms.local',N'AKILA.GREEN@OCVMS.LOCAL',N'akila.green@ocvms.local',N'AKILA.GREEN@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'558a1d44-0697-45c7-82ac-b3cc9ec69906',N'84c2a8db-c845-41b3-9272-c23dc0715980',NULL,1,0,NULL,1,0),
(N'user-org-life-01',N'malki.lifecare@ocvms.local',N'MALKI.LIFECARE@OCVMS.LOCAL',N'malki.lifecare@ocvms.local',N'MALKI.LIFECARE@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'718cc6d2-7674-4978-a46b-65c8542b9b9a',N'd56a7f40-ae15-48db-b5ed-1b0957d5dea3',NULL,1,0,NULL,1,0),
(N'user-org-life-02',N'dilshan.lifecare@ocvms.local',N'DILSHAN.LIFECARE@OCVMS.LOCAL',N'dilshan.lifecare@ocvms.local',N'DILSHAN.LIFECARE@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'7aa20f0c-b482-40e1-9df6-bd90772fc5d2',N'808bde20-ee6f-43ba-abbc-09e7cadd6fe0',NULL,1,0,NULL,1,0),
(N'user-org-bright-01',N'ravindu.bright@ocvms.local',N'RAVINDU.BRIGHT@OCVMS.LOCAL',N'ravindu.bright@ocvms.local',N'RAVINDU.BRIGHT@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'5a684ac1-0d41-4b4c-8c9e-599da0fc7383',N'4c84c70e-6fb2-49a6-8764-72b96ceac3bf',NULL,1,0,NULL,1,0),
(N'user-org-bright-02',N'ishara.bright@ocvms.local',N'ISHARA.BRIGHT@OCVMS.LOCAL',N'ishara.bright@ocvms.local',N'ISHARA.BRIGHT@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'c6f09b11-1fe3-433a-b420-c56ab2e8a9ba',N'b199aad4-d3a2-424f-838b-10ef1dc5b521',NULL,1,0,NULL,1,0),
(N'user-vol-01',N'volunteer01@ocvms.local',N'VOLUNTEER01@OCVMS.LOCAL',N'volunteer01@ocvms.local',N'VOLUNTEER01@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'ae44a470-55ca-4001-87a0-8e2b345eed3c',N'268dd8d4-d550-4b88-b474-80b7769c5a5e',NULL,1,0,NULL,1,0),
(N'user-vol-02',N'volunteer02@ocvms.local',N'VOLUNTEER02@OCVMS.LOCAL',N'volunteer02@ocvms.local',N'VOLUNTEER02@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'860ed4b0-8e1e-465b-87f8-8edfbc123c67',N'6a4ed2e0-eefc-40fe-9131-957ca88ffc47',NULL,1,0,NULL,1,0),
(N'user-vol-03',N'volunteer03@ocvms.local',N'VOLUNTEER03@OCVMS.LOCAL',N'volunteer03@ocvms.local',N'VOLUNTEER03@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'21978bf6-8194-4805-8676-c5256471f7e2',N'6fd18f82-38cc-4827-b3af-82a920181fca',NULL,1,0,NULL,1,0),
(N'user-vol-04',N'volunteer04@ocvms.local',N'VOLUNTEER04@OCVMS.LOCAL',N'volunteer04@ocvms.local',N'VOLUNTEER04@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'50c473cd-a6cd-4e7a-a9ca-0238088a6b4b',N'aef84a74-47ed-4920-974e-5878fe7ce09f',NULL,1,0,NULL,1,0),
(N'user-vol-05',N'volunteer05@ocvms.local',N'VOLUNTEER05@OCVMS.LOCAL',N'volunteer05@ocvms.local',N'VOLUNTEER05@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'909d6d04-f9f5-4974-96d8-5b04ce567abf',N'30cdbd3f-5575-4cdb-8b85-b3139aa46396',NULL,1,0,NULL,1,0),
(N'user-vol-06',N'volunteer06@ocvms.local',N'VOLUNTEER06@OCVMS.LOCAL',N'volunteer06@ocvms.local',N'VOLUNTEER06@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'bebe749c-280a-445d-a5da-21f554a4cfde',N'11e61787-1126-4a35-81e2-e79a1a7d1a8f',NULL,1,0,NULL,1,0),
(N'user-vol-07',N'volunteer07@ocvms.local',N'VOLUNTEER07@OCVMS.LOCAL',N'volunteer07@ocvms.local',N'VOLUNTEER07@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'bba3a2dd-e457-4234-8a3d-bcdddd5abcec',N'4cab4505-cf44-4c54-aee8-11219c96e725',NULL,1,0,NULL,1,0),
(N'user-vol-08',N'volunteer08@ocvms.local',N'VOLUNTEER08@OCVMS.LOCAL',N'volunteer08@ocvms.local',N'VOLUNTEER08@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'9ca1183f-3a59-47a9-8ff8-4b8336705f07',N'986d5a3f-e49e-4eac-baf8-7e78577c1a6f',NULL,1,0,NULL,1,0),
(N'user-vol-09',N'volunteer09@ocvms.local',N'VOLUNTEER09@OCVMS.LOCAL',N'volunteer09@ocvms.local',N'VOLUNTEER09@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'5d0bc193-ef18-4b88-b2db-a18d4bd7f6a8',N'ac484155-89d6-4d29-8ba3-17b5840fa715',NULL,1,0,NULL,1,0),
(N'user-vol-10',N'volunteer10@ocvms.local',N'VOLUNTEER10@OCVMS.LOCAL',N'volunteer10@ocvms.local',N'VOLUNTEER10@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'a3407536-114d-4e9e-b2bf-c59661a18ea5',N'922071d8-42d2-4f1c-8cca-d105d84f9dc6',NULL,1,0,NULL,1,0),
(N'user-vol-11',N'volunteer11@ocvms.local',N'VOLUNTEER11@OCVMS.LOCAL',N'volunteer11@ocvms.local',N'VOLUNTEER11@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'6326a07a-9ef5-4059-9266-e8e6b5fef09c',N'baeeb4c6-d6a2-4859-8354-381468525921',NULL,1,0,NULL,1,0),
(N'user-vol-12',N'volunteer12@ocvms.local',N'VOLUNTEER12@OCVMS.LOCAL',N'volunteer12@ocvms.local',N'VOLUNTEER12@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPHolPEEzUiKFQvnVqDxsbLilbsGmDrFM1jw6OnIkHb8QuEsslU2Kd8jwE5WL/dD7A==',N'e49f23f6-cda4-4b51-a196-0664d04986c4',N'049fe59e-c1ff-4b9c-8bec-2426d25de1bc',NULL,1,0,DATEADD(year, 10, SYSDATETIMEOFFSET()),1,0);

INSERT INTO [AspNetUserRoles] ([UserId],[RoleId]) VALUES
(N'user-admin',N'role-admin'),
(N'user-org-green-01',N'role-organizer'),
(N'user-org-green-02',N'role-organizer'),
(N'user-org-life-01',N'role-organizer'),
(N'user-org-life-02',N'role-organizer'),
(N'user-org-bright-01',N'role-organizer'),
(N'user-org-bright-02',N'role-organizer'),
(N'user-vol-01',N'role-volunteer'),
(N'user-vol-02',N'role-volunteer'),
(N'user-vol-03',N'role-volunteer'),
(N'user-vol-04',N'role-volunteer'),
(N'user-vol-05',N'role-volunteer'),
(N'user-vol-06',N'role-volunteer'),
(N'user-vol-07',N'role-volunteer'),
(N'user-vol-08',N'role-volunteer'),
(N'user-vol-09',N'role-volunteer'),
(N'user-vol-10',N'role-volunteer'),
(N'user-vol-11',N'role-volunteer'),
(N'user-vol-12',N'role-volunteer');

SET IDENTITY_INSERT [UserProfiles] ON;
INSERT INTO [UserProfiles] ([Id],[UserId],[FullName],[RoleName],[PublicEmail],[ContactNumber],[Bio],[Skills],[Availability],[ProfileImageUrl],[OrganizationName],[IsVerified],[CreatedAt]) VALUES
(1,N'user-admin',N'System Administrator',N'Admin',N'admin@ocvms.local',N'0710000000',N'System administrator account for OCVMS final demonstration.',N'Platform administration, user verification, account control',N'Available during project demonstration',N'/uploads/profiles/profile_admin.png',NULL,1,DATEADD(day,-45,GETUTCDATE())),
(2,N'user-org-green-01',N'Nishan Perera',N'Organizer',N'nishan.green@ocvms.local',N'0771100101',N'Organizer coordinating environmental and community service activities.',N'Event planning, volunteer coordination, public communication',N'Weekends and weekday evenings',N'/uploads/profiles/profile_green_org1.png',N'Green Hope Foundation',1,DATEADD(day,-38,GETUTCDATE())),
(3,N'user-org-green-02',N'Akila Kariyawasam',N'Organizer',N'akila.green@ocvms.local',N'0771100102',N'Organizer supporting green projects and youth volunteer programs.',N'Logistics management, awareness campaigns, resource coordination',N'Weekends morning',N'/uploads/profiles/profile_green_org2.png',N'Green Hope Foundation',1,DATEADD(day,-37,GETUTCDATE())),
(4,N'user-org-life-01',N'Malki Senanayake',N'Organizer',N'malki.lifecare@ocvms.local',N'0772200201',N'Healthcare volunteer organizer focused on community welfare programs.',N'Health camp coordination, first aid awareness, community outreach',N'Flexible schedule',N'/uploads/profiles/profile_lifecare_org1.png',N'LifeCare Volunteers',1,DATEADD(day,-36,GETUTCDATE())),
(5,N'user-org-life-02',N'Dilshan Rathnayake',N'Organizer',N'dilshan.lifecare@ocvms.local',N'0772200202',N'Organizer supporting elderly care and public health volunteer events.',N'Volunteer scheduling, medical camp support, transport coordination',N'Weekday evenings',N'/uploads/profiles/profile_lifecare_org2.png',N'LifeCare Volunteers',1,DATEADD(day,-35,GETUTCDATE())),
(6,N'user-org-bright-01',N'Ravindu Wijesinghe',N'Organizer',N'ravindu.bright@ocvms.local',N'0773300301',N'Organizer for educational support and youth development programs.',N'Mentoring, education support, school donation coordination',N'Weekends full day',N'/uploads/profiles/profile_bright_org1.png',N'Bright Future Sri Lanka',1,DATEADD(day,-34,GETUTCDATE())),
(7,N'user-org-bright-02',N'Ishara Nanayakkara',N'Organizer',N'ishara.bright@ocvms.local',N'0773300302',N'Organizer supporting youth career guidance and learning workshops.',N'Career guidance, child education, event communication',N'Public holidays and weekends',N'/uploads/profiles/profile_bright_org2.png',N'Bright Future Sri Lanka',1,DATEADD(day,-33,GETUTCDATE())),
(8,N'user-vol-01',N'Sanduni Gunasekara',N'Volunteer',N'volunteer01@ocvms.local',N'076440001',N'Community volunteer interested in meaningful service activities and public events.',N'First aid, event support, crowd coordination',N'Weekends morning',N'/uploads/profiles/profile_volunteer_01.png',N'Independent Volunteer',1,DATEADD(day,-29,GETUTCDATE())),
(9,N'user-vol-02',N'Kasun Abeysekara',N'Volunteer',N'volunteer02@ocvms.local',N'076440002',N'Community volunteer interested in meaningful service activities and public events.',N'Teaching, mentoring, communication',N'Weekends full day',N'/uploads/profiles/profile_volunteer_02.png',N'Independent Volunteer',1,DATEADD(day,-28,GETUTCDATE())),
(10,N'user-vol-03',N'Lahiru Perera',N'Volunteer',N'volunteer03@ocvms.local',N'076440003',N'Community volunteer interested in meaningful service activities and public events.',N'Photography, social media, event documentation',N'Weekday evenings',N'/uploads/profiles/profile_volunteer_03.png',N'Independent Volunteer',1,DATEADD(day,-27,GETUTCDATE())),
(11,N'user-vol-04',N'Tharushi Wickramasinghe',N'Volunteer',N'volunteer04@ocvms.local',N'076440004',N'Community volunteer interested in meaningful service activities and public events.',N'Logistics, transport support, resource handling',N'Public holidays',N'/uploads/profiles/profile_volunteer_04.png',N'Independent Volunteer',1,DATEADD(day,-26,GETUTCDATE())),
(12,N'user-vol-05',N'Naveen Madushanka',N'Volunteer',N'volunteer05@ocvms.local',N'076440005',N'Community volunteer interested in meaningful service activities and public events.',N'Cleaning campaigns, environmental awareness',N'Flexible schedule',N'/uploads/profiles/profile_volunteer_05.png',N'Independent Volunteer',0,DATEADD(day,-25,GETUTCDATE())),
(13,N'user-vol-06',N'Oshadi Silva',N'Volunteer',N'volunteer06@ocvms.local',N'076440006',N'Community volunteer interested in meaningful service activities and public events.',N'Food distribution, donation handling, teamwork',N'Weekends morning',N'/uploads/profiles/profile_volunteer_06.png',N'Independent Volunteer',1,DATEADD(day,-24,GETUTCDATE())),
(14,N'user-vol-07',N'Vihanga Gamage',N'Volunteer',N'volunteer07@ocvms.local',N'076440007',N'Community volunteer interested in meaningful service activities and public events.',N'Community outreach, registration desk, public support',N'Weekends full day',N'/uploads/profiles/profile_volunteer_07.png',N'Independent Volunteer',1,DATEADD(day,-23,GETUTCDATE())),
(15,N'user-vol-08',N'Chamodi Nirmala',N'Volunteer',N'volunteer08@ocvms.local',N'076440008',N'Community volunteer interested in meaningful service activities and public events.',N'Children''s activities, reading support, mentoring',N'Weekday evenings',N'/uploads/profiles/profile_volunteer_08.png',N'Independent Volunteer',1,DATEADD(day,-22,GETUTCDATE())),
(16,N'user-vol-09',N'Yasiru Pathirana',N'Volunteer',N'volunteer09@ocvms.local',N'076440009',N'Community volunteer interested in meaningful service activities and public events.',N'First aid, emergency response, safety guidance',N'Public holidays',N'/uploads/profiles/profile_volunteer_09.png',N'Independent Volunteer',0,DATEADD(day,-21,GETUTCDATE())),
(17,N'user-vol-10',N'Piumi Bandara',N'Volunteer',N'volunteer10@ocvms.local',N'076440010',N'Community volunteer interested in meaningful service activities and public events.',N'Donation collection, inventory support, communication',N'Flexible schedule',N'/uploads/profiles/profile_volunteer_10.png',N'Independent Volunteer',1,DATEADD(day,-20,GETUTCDATE())),
(18,N'user-vol-11',N'Ruwan Dissanayake',N'Volunteer',N'volunteer11@ocvms.local',N'076440011',N'Community volunteer interested in meaningful service activities and public events.',N'Environmental campaigns, team coordination, cleanup support',N'Weekends morning',N'/uploads/profiles/profile_volunteer_11.png',N'Independent Volunteer',1,DATEADD(day,-19,GETUTCDATE())),
(19,N'user-vol-12',N'Hiruni Nethmini',N'Volunteer',N'volunteer12@ocvms.local',N'076440012',N'Community volunteer interested in meaningful service activities and public events.',N'Volunteer support, help desk, social media updates',N'Weekends full day',N'/uploads/profiles/profile_volunteer_12.png',N'Independent Volunteer',1,DATEADD(day,-18,GETUTCDATE()));
SET IDENTITY_INSERT [UserProfiles] OFF;

SET IDENTITY_INSERT [VolunteerEvents] ON;
INSERT INTO [VolunteerEvents] ([Id],[Title],[Description],[Location],[EventDate],[Capacity],[Status],[ImageUrl],[OrganizerProfileId],[CreatedAt],[EventTime],[RegistrationOpenDate],[RegistrationClosingDate]) VALUES
(1,N'Community Blood Donation Campaign',N'A well-organized blood donation campaign to support local hospitals and emergency blood requirements. Volunteers will assist with registration, queue coordination, refreshments, and donor guidance.',N'Colombo Community Hall',DATEADD(day,10,CAST(GETDATE() AS date)),70,N'Upcoming',N'/uploads/events/event_blood_donation.png',2,DATEADD(day,-18,GETUTCDATE()),CAST(N'09:00:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,7,CAST(GETDATE() AS date))),
(2,N'Beach Cleaning and Plastic Awareness Drive',N'Community cleanup event focused on reducing plastic waste and creating awareness among families and youth groups in the coastal area.',N'Galle Face Green',DATEADD(day,12,CAST(GETDATE() AS date)),80,N'Upcoming',N'/uploads/events/event_beach_cleanup.png',3,DATEADD(day,-17,GETUTCDATE()),CAST(N'07:30:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,9,CAST(GETDATE() AS date))),
(3,N'Tree Planting and Green City Project',N'Tree planting project to improve public green spaces. Volunteers will help with planting, watering, labeling, and awareness activities.',N'Viharamahadevi Park',DATEADD(day,15,CAST(GETDATE() AS date)),60,N'Upcoming',N'/uploads/events/event_tree_planting.png',2,DATEADD(day,-16,GETUTCDATE()),CAST(N'08:00:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,10,CAST(GETDATE() AS date))),
(4,N'Free Medical Screening Camp',N'A free health screening camp for low-income families, including basic checkups, registration support, and crowd guidance.',N'Kandy Municipal Hall',DATEADD(day,14,CAST(GETDATE() AS date)),50,N'Upcoming',N'/uploads/events/event_medical_camp.png',4,DATEADD(day,-15,GETUTCDATE()),CAST(N'08:30:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,8,CAST(GETDATE() AS date))),
(5,N'Elderly Care Home Support Day',N'A volunteer support day at an elderly care home with cleaning, companionship activities, meal support, and basic facility organization.',N'Maharagama Elderly Care Center',DATEADD(day,18,CAST(GETDATE() AS date)),40,N'Upcoming',N'/uploads/events/event_elder_care.png',5,DATEADD(day,-14,GETUTCDATE()),CAST(N'10:00:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,10,CAST(GETDATE() AS date))),
(6,N'Community First Aid Awareness Program',N'Training and awareness event to educate community members about first aid basics and emergency response methods.',N'Negombo Public Ground',DATEADD(day,20,CAST(GETDATE() AS date)),55,N'Upcoming',N'/uploads/events/event_first_aid.png',4,DATEADD(day,-13,GETUTCDATE()),CAST(N'09:30:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,12,CAST(GETDATE() AS date))),
(7,N'School Supplies Distribution Program',N'Donation program to distribute school supplies to children. Volunteers will assist with packing, verification, and distribution.',N'Kurunegala District School Hall',DATEADD(day,16,CAST(GETDATE() AS date)),65,N'Upcoming',N'/uploads/events/event_school_supplies.png',6,DATEADD(day,-12,GETUTCDATE()),CAST(N'08:45:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,9,CAST(GETDATE() AS date))),
(8,N'Children''s Reading and Learning Workshop',N'A workshop to improve reading habits among children through story sessions, reading games, and group learning activities.',N'Gampaha Public Library',DATEADD(day,22,CAST(GETDATE() AS date)),35,N'Upcoming',N'/uploads/events/event_reading_workshop.png',7,DATEADD(day,-11,GETUTCDATE()),CAST(N'10:30:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,11,CAST(GETDATE() AS date))),
(9,N'Youth Career Guidance and Mentoring Session',N'Career guidance session for students and young adults with mentoring, CV advice, and skill-sharing activities.',N'Moratuwa Youth Center',DATEADD(day,25,CAST(GETDATE() AS date)),45,N'Upcoming',N'/uploads/events/event_career_guidance.png',6,DATEADD(day,-10,GETUTCDATE()),CAST(N'13:00:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,12,CAST(GETDATE() AS date))),
(10,N'Food Donation Drive for Low-Income Families',N'Food donation packing and distribution program for selected low-income families in the community.',N'Dehiwala Community Kitchen',DATEADD(day,11,CAST(GETDATE() AS date)),50,N'Upcoming',N'/uploads/events/event_food_donation.png',3,DATEADD(day,-9,GETUTCDATE()),CAST(N'09:00:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,7,CAST(GETDATE() AS date))),
(11,N'Community Recycling Collection Weekend',N'Weekend recycling collection campaign to promote waste sorting and responsible disposal among local households.',N'Nugegoda Public Car Park',DATEADD(day,17,CAST(GETDATE() AS date)),75,N'Upcoming',N'/uploads/events/event_recycling.png',2,DATEADD(day,-8,GETUTCDATE()),CAST(N'08:00:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,8,CAST(GETDATE() AS date))),
(12,N'Disaster Relief Packing Volunteer Day',N'Volunteer day to pack emergency relief items for families affected by floods and severe weather conditions.',N'Ratnapura Relief Center',DATEADD(day,21,CAST(GETDATE() AS date)),90,N'Upcoming',N'/uploads/events/event_disaster_relief.png',5,DATEADD(day,-7,GETUTCDATE()),CAST(N'07:45:00' AS time),DATEADD(day,0,CAST(GETDATE() AS date)),DATEADD(day,14,CAST(GETDATE() AS date)));
SET IDENTITY_INSERT [VolunteerEvents] OFF;

SET IDENTITY_INSERT [EventRegistrations] ON;
INSERT INTO [EventRegistrations] ([Id],[VolunteerEventId],[UserId],[RegistrationDate],[CreatedAt]) VALUES
(1,1,N'user-vol-01',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(2,1,N'user-vol-02',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(3,1,N'user-vol-03',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(4,1,N'user-vol-04',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(5,1,N'user-vol-05',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(6,1,N'user-vol-06',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(7,2,N'user-vol-02',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(8,2,N'user-vol-03',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(9,2,N'user-vol-04',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(10,2,N'user-vol-05',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(11,2,N'user-vol-06',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(12,2,N'user-vol-07',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(13,3,N'user-vol-03',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(14,3,N'user-vol-04',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(15,3,N'user-vol-05',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(16,3,N'user-vol-06',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(17,3,N'user-vol-07',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(18,3,N'user-vol-08',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(19,4,N'user-vol-04',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(20,4,N'user-vol-05',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(21,4,N'user-vol-06',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(22,4,N'user-vol-07',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(23,4,N'user-vol-08',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(24,4,N'user-vol-09',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(25,5,N'user-vol-05',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(26,5,N'user-vol-06',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(27,5,N'user-vol-07',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(28,5,N'user-vol-08',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(29,5,N'user-vol-09',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(30,5,N'user-vol-10',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(31,6,N'user-vol-06',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(32,6,N'user-vol-07',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(33,6,N'user-vol-08',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(34,6,N'user-vol-09',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(35,6,N'user-vol-10',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(36,6,N'user-vol-11',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(37,7,N'user-vol-07',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(38,7,N'user-vol-08',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(39,7,N'user-vol-09',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(40,7,N'user-vol-10',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(41,7,N'user-vol-11',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(42,7,N'user-vol-12',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(43,8,N'user-vol-08',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(44,8,N'user-vol-09',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(45,8,N'user-vol-10',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(46,8,N'user-vol-11',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(47,8,N'user-vol-12',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(48,8,N'user-vol-01',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(49,9,N'user-vol-09',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(50,9,N'user-vol-10',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(51,9,N'user-vol-11',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(52,9,N'user-vol-12',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(53,9,N'user-vol-01',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(54,9,N'user-vol-02',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(55,10,N'user-vol-10',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(56,10,N'user-vol-11',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(57,10,N'user-vol-12',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(58,10,N'user-vol-01',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(59,10,N'user-vol-02',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(60,10,N'user-vol-03',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(61,11,N'user-vol-11',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(62,11,N'user-vol-12',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(63,11,N'user-vol-01',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(64,11,N'user-vol-02',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(65,11,N'user-vol-03',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(66,11,N'user-vol-04',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(67,12,N'user-vol-12',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(68,12,N'user-vol-01',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(69,12,N'user-vol-02',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(70,12,N'user-vol-03',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(71,12,N'user-vol-04',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(72,12,N'user-vol-05',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE()));
SET IDENTITY_INSERT [EventRegistrations] OFF;

SET IDENTITY_INSERT [UserRatings] ON;
INSERT INTO [UserRatings] ([Id],[FromUserId],[ToUserId],[EventId],[Score],[ReviewText],[CreatedAt]) VALUES
(1,N'user-vol-01',N'user-org-green-01',1,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(2,N'user-vol-02',N'user-org-green-01',1,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(3,N'user-vol-03',N'user-org-green-01',1,5,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(4,N'user-vol-04',N'user-org-green-01',1,4,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(5,N'user-vol-02',N'user-org-green-02',2,4,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(6,N'user-vol-03',N'user-org-green-02',2,5,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(7,N'user-vol-04',N'user-org-green-02',2,4,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(8,N'user-vol-05',N'user-org-green-02',2,5,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(9,N'user-vol-03',N'user-org-green-01',3,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(10,N'user-vol-04',N'user-org-green-01',3,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(11,N'user-vol-05',N'user-org-green-01',3,5,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(12,N'user-vol-06',N'user-org-green-01',3,4,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(13,N'user-vol-04',N'user-org-life-01',4,4,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(14,N'user-vol-05',N'user-org-life-01',4,5,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(15,N'user-vol-06',N'user-org-life-01',4,4,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(16,N'user-vol-07',N'user-org-life-01',4,5,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(17,N'user-vol-05',N'user-org-life-02',5,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(18,N'user-vol-06',N'user-org-life-02',5,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(19,N'user-vol-07',N'user-org-life-02',5,5,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(20,N'user-vol-08',N'user-org-life-02',5,4,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(21,N'user-vol-06',N'user-org-life-01',6,4,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(22,N'user-vol-07',N'user-org-life-01',6,5,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(23,N'user-vol-08',N'user-org-life-01',6,4,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(24,N'user-vol-09',N'user-org-life-01',6,5,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(25,N'user-vol-07',N'user-org-bright-01',7,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(26,N'user-vol-08',N'user-org-bright-01',7,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(27,N'user-vol-09',N'user-org-bright-01',7,5,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(28,N'user-vol-10',N'user-org-bright-01',7,4,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(29,N'user-vol-08',N'user-org-bright-02',8,4,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(30,N'user-vol-09',N'user-org-bright-02',8,5,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(31,N'user-vol-10',N'user-org-bright-02',8,4,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(32,N'user-vol-11',N'user-org-bright-02',8,5,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(33,N'user-vol-09',N'user-org-bright-01',9,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(34,N'user-vol-10',N'user-org-bright-01',9,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(35,N'user-vol-11',N'user-org-bright-01',9,5,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(36,N'user-vol-12',N'user-org-bright-01',9,4,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(37,N'user-vol-10',N'user-org-green-02',10,4,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(38,N'user-vol-11',N'user-org-green-02',10,5,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(39,N'user-vol-12',N'user-org-green-02',10,4,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(40,N'user-vol-01',N'user-org-green-02',10,5,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(41,N'user-vol-11',N'user-org-green-01',11,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(42,N'user-vol-12',N'user-org-green-01',11,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(43,N'user-vol-01',N'user-org-green-01',11,5,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(44,N'user-vol-02',N'user-org-green-01',11,4,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE())),
(45,N'user-vol-12',N'user-org-life-02',12,4,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(46,N'user-vol-01',N'user-org-life-02',12,5,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(47,N'user-vol-02',N'user-org-life-02',12,4,N'A very useful project for the community.',DATEADD(day,-4,GETUTCDATE())),
(48,N'user-vol-03',N'user-org-life-02',12,5,N'Good coordination and volunteer support.',DATEADD(day,-5,GETUTCDATE()));
SET IDENTITY_INSERT [UserRatings] OFF;

SET IDENTITY_INSERT [CommunityPosts] ON;
INSERT INTO [CommunityPosts] ([Id],[Title],[PostType],[Content],[UserProfileId],[CreatedAt]) VALUES
(1,N'Welcome to the OCVMS community',N'Share',N'This platform helps volunteers, organizers, and admins coordinate meaningful community service activities through events, posts, support requests, and notifications.',1,DATEADD(day,-6,GETUTCDATE())),
(2,N'Volunteers needed for beach cleanup',N'Announcement',N'Green Hope Foundation is preparing a beach cleanup activity. Volunteers who are interested in environmental work can join and support the campaign.',2,DATEADD(day,-5,GETUTCDATE())),
(3,N'Request for school supply donations',N'Help',N'Bright Future Sri Lanka is collecting books, pencils, bags, and other useful school items for children who need educational support.',6,DATEADD(day,-4,GETUTCDATE())),
(4,N'First aid awareness session completed successfully',N'Share',N'LifeCare Volunteers successfully completed a first aid awareness session with strong support from registered volunteers.',4,DATEADD(day,-3,GETUTCDATE())),
(5,N'Transport support needed for food donation event',N'Help',N'We need two vehicles to support the upcoming food donation drive. Volunteers who can help with transport are welcome to contact the organizer.',3,DATEADD(day,-2,GETUTCDATE())),
(6,N'Thank you to all active volunteers',N'Share',N'A special thank you to all volunteers who joined recent events and helped make each activity successful.',1,DATEADD(day,-1,GETUTCDATE()));
SET IDENTITY_INSERT [CommunityPosts] OFF;

SET IDENTITY_INSERT [PostComments] ON;
INSERT INTO [PostComments] ([Id],[CommunityPostId],[UserProfileId],[Content],[CreatedAt]) VALUES
(1,2,8,N'I can help with registration and waste collection.',DATEADD(day,-4,GETUTCDATE())),
(2,2,12,N'Please share the reporting time for volunteers.',DATEADD(day,-4,GETUTCDATE())),
(3,3,9,N'I can donate exercise books and pencils.',DATEADD(day,-3,GETUTCDATE())),
(4,4,10,N'It was a very useful session for the community.',DATEADD(day,-2,GETUTCDATE())),
(5,5,11,N'I can help with loading and unloading items.',DATEADD(day,-1,GETUTCDATE()));
SET IDENTITY_INSERT [PostComments] OFF;

SET IDENTITY_INSERT [HelpRequests] ON;
INSERT INTO [HelpRequests] ([Id],[Title],[Description],[Status],[UserProfileId],[CreatedAt]) VALUES
(1,N'Need volunteers for elderly care support',N'We need 5 volunteers to assist with cleaning and companionship activities at the elderly care center.',N'Pending',5,DATEADD(day,-5,GETUTCDATE())),
(2,N'Transport support for donation items',N'A small van or car is needed to transport packed donation items to the distribution location.',N'In Progress',3,DATEADD(day,-4,GETUTCDATE())),
(3,N'Need extra books for reading workshop',N'Children''s reading workshop needs story books and basic stationery items.',N'Pending',7,DATEADD(day,-3,GETUTCDATE())),
(4,N'Medical camp registration desk help',N'LifeCare Volunteers need two people to help with registration desk management.',N'Completed',4,DATEADD(day,-2,GETUTCDATE()));
SET IDENTITY_INSERT [HelpRequests] OFF;

SET IDENTITY_INSERT [Notifications] ON;
INSERT INTO [Notifications] ([Id],[UserProfileId],[Message],[IsRead],[CreatedAt]) VALUES
(1,2,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-8,GETUTCDATE())),
(2,3,N'New volunteer registrations are available for your beach cleanup event.',0,DATEADD(day,-6,GETUTCDATE())),
(3,8,N'You have successfully registered for Community Blood Donation Campaign.',1,DATEADD(day,-5,GETUTCDATE())),
(4,10,N'Reminder: Please check event details before attending.',0,DATEADD(day,-4,GETUTCDATE())),
(5,6,N'Your organization rating has been updated after recent event feedback.',0,DATEADD(day,-3,GETUTCDATE())),
(6,19,N'Your account is currently suspended for demonstration of admin ban feature.',0,DATEADD(day,-1,GETUTCDATE()));
SET IDENTITY_INSERT [Notifications] OFF;


COMMIT TRANSACTION;

SELECT 'AspNetUsers' AS TableName, COUNT(*) AS TotalCount FROM AspNetUsers
UNION ALL SELECT 'UserProfiles', COUNT(*) FROM UserProfiles
UNION ALL SELECT 'VolunteerEvents', COUNT(*) FROM VolunteerEvents
UNION ALL SELECT 'EventRegistrations', COUNT(*) FROM EventRegistrations
UNION ALL SELECT 'UserRatings', COUNT(*) FROM UserRatings
UNION ALL SELECT 'CommunityPosts', COUNT(*) FROM CommunityPosts
UNION ALL SELECT 'HelpRequests', COUNT(*) FROM HelpRequests
UNION ALL SELECT 'Notifications', COUNT(*) FROM Notifications;

'@

Write-Host "Resetting OCVMSDb and inserting full demo data..." -ForegroundColor Cyan
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$cmd = $conn.CreateCommand()
$cmd.CommandText = $sql
$cmd.CommandTimeout = 600

$conn.Open()
$reader = $cmd.ExecuteReader()
$table = New-Object System.Data.DataTable
$table.Load($reader)
$conn.Close()

Write-Host "`nDemo data inserted successfully. Counts:" -ForegroundColor Green
$table | Format-Table -AutoSize

$masterConnectionString = "Server=$server;Database=master;Trusted_Connection=True;TrustServerCertificate=True"
$backupSql = @"
BACKUP DATABASE [$database]
TO DISK = N'$backupPath'
WITH INIT, FORMAT, NAME = N'OCVMS Full Demo Data Backup';
"@

Write-Host "`nCreating backup file: $backupPath" -ForegroundColor Cyan
$conn = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
$cmd = $conn.CreateCommand()
$cmd.CommandText = $backupSql
$cmd.CommandTimeout = 600
$conn.Open()
$cmd.ExecuteNonQuery() | Out-Null
$conn.Close()

Write-Host "`nDONE! Full demo data backup created at: $backupPath" -ForegroundColor Green
Write-Host "Login accounts:" -ForegroundColor Yellow
Write-Host "Admin:      admin@ocvms.local / Admin@2026#"
Write-Host "Organizer:  nishan.green@ocvms.local / User@123"
Write-Host "Organizer:  malki.lifecare@ocvms.local / User@123"
Write-Host "Volunteer:  volunteer01@ocvms.local / User@123"
Write-Host "Banned demo user: volunteer12@ocvms.local / User@123"
