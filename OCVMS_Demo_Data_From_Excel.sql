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

IF OBJECT_ID(N'dbo.UserRatings', N'U') IS NOT NULL DELETE FROM [UserRatings];

IF OBJECT_ID(N'dbo.PostComments', N'U') IS NOT NULL DELETE FROM [PostComments];

IF OBJECT_ID(N'dbo.EventRegistrations', N'U') IS NOT NULL DELETE FROM [EventRegistrations];

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NOT NULL DELETE FROM [Notifications];

IF OBJECT_ID(N'dbo.HelpRequests', N'U') IS NOT NULL DELETE FROM [HelpRequests];

IF OBJECT_ID(N'dbo.CommunityPosts', N'U') IS NOT NULL DELETE FROM [CommunityPosts];

IF OBJECT_ID(N'dbo.VolunteerEvents', N'U') IS NOT NULL DELETE FROM [VolunteerEvents];

IF OBJECT_ID(N'dbo.UserProfiles', N'U') IS NOT NULL DELETE FROM [UserProfiles];

IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL DELETE FROM [AspNetUserClaims];

IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL DELETE FROM [AspNetUserLogins];

IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NOT NULL DELETE FROM [AspNetUserTokens];

IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL DELETE FROM [AspNetUserRoles];

IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL DELETE FROM [AspNetUsers];

IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NOT NULL DELETE FROM [AspNetRoleClaims];

IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NOT NULL DELETE FROM [AspNetRoles];


INSERT INTO [AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp]) VALUES
(N'role-admin',N'Admin',N'ADMIN',N'70c6e245-aba6-4b62-88c4-3e6b8a74827a'),
(N'role-organizer',N'Organizer',N'ORGANIZER',N'791a604e-325e-4f82-9aed-f76ebea35d44'),
(N'role-volunteer',N'Volunteer',N'VOLUNTEER',N'8673074b-2ad7-4209-b1cf-a93d0919c8b0');


INSERT INTO [AspNetUsers] ([Id],[UserName],[NormalizedUserName],[Email],[NormalizedEmail],[EmailConfirmed],[PasswordHash],[SecurityStamp],[ConcurrencyStamp],[PhoneNumber],[PhoneNumberConfirmed],[TwoFactorEnabled],[LockoutEnd],[LockoutEnabled],[AccessFailedCount]) VALUES
(N'user-admin',N'admin@ocvms.local',N'ADMIN@OCVMS.LOCAL',N'admin@ocvms.local',N'ADMIN@OCVMS.LOCAL',1,N'AQAAAAEAAYagAAAAEPWp4tic9ZUgAGD/nHmFGcli+h4sPzKFBxCCEELUXYwP1/nTmZr9du+N8eqZqkxsFQ==',N'0e59b329-bc06-4a03-b311-2f432ea99da7',N'4ca15c0a-f086-4ee8-8f9f-b958ddf604a4',NULL,1,0,NULL,1,0),
(N'user-org-01',N'kasun.perera92@gmail.com',N'KASUN.PERERA92@GMAIL.COM',N'kasun.perera92@gmail.com',N'KASUN.PERERA92@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEE/D7b3hnuCQkV1qTK0nWuvGVuodl+2lRt0bLCqpCbKZR2HrCEzKZ4UdTgvMNnBILw==',N'9cdda0c1-a13e-45d9-b8d2-84924cac1c90',N'341097c5-d49b-4ab5-a035-281f50b6b7e1',NULL,1,0,NULL,1,0),
(N'user-org-02',N'nimali.fernando88@gmail.com',N'NIMALI.FERNANDO88@GMAIL.COM',N'nimali.fernando88@gmail.com',N'NIMALI.FERNANDO88@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEJ+BDnu7FLibXldsP8usczQft4pGCJf1r52is2kijesmFWPFxYcuN+eL7r7GdnEHbw==',N'a2351fdf-2ec0-41ad-bc1b-4a2d97c9e879',N'7baf21d7-d196-450d-b02b-971babb08e4d',NULL,1,0,NULL,1,0),
(N'user-org-03',N'dasunsilva.lk@gmail.com',N'DASUNSILVA.LK@GMAIL.COM',N'dasunsilva.lk@gmail.com',N'DASUNSILVA.LK@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEMShn7M0TbpF2QGCv6704Bvb2kB9FFXSscTigpCqDq5xsumYWIDvPSYkRzefgR4S5g==',N'98170878-49f0-430a-b455-c2d0aefd3c3f',N'71175f3b-1a2e-4c03-952f-eac535929a19',NULL,1,0,NULL,1,0),
(N'user-org-04',N'sanduni.jaya@gmail.com',N'SANDUNI.JAYA@GMAIL.COM',N'sanduni.jaya@gmail.com',N'SANDUNI.JAYA@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEFX+jqgMJaewCGmgIPY1QCZrWRq4hDtX32MkKjLCcZDJPg273zJzUt8ZfzOkttkaqg==',N'484e1987-9d9c-4e95-af22-26c242636f8d',N'6f21a1ae-fc43-4a7f-95a0-21c4a7a2c8ec',NULL,1,0,NULL,1,0),
(N'user-org-05',N'lahiru.bandara95@gmail.com',N'LAHIRU.BANDARA95@GMAIL.COM',N'lahiru.bandara95@gmail.com',N'LAHIRU.BANDARA95@GMAIL.COM',1,N'AQAAAAEAAYagAAAAELeXN74ilk0S1/NeVGfja2IJ9hz5hwbTaYkldD6n0tIxXAv74uIXuuRMSMV+vOD/eQ==',N'fb0871a6-847c-4ece-92a1-22afcdec11e8',N'c13ec7fa-191c-478f-bc5a-7f0fe5b3a9ae',NULL,1,0,NULL,1,0),
(N'user-org-06',N'tharushi.rathnayake@gmail.com',N'THARUSHI.RATHNAYAKE@GMAIL.COM',N'tharushi.rathnayake@gmail.com',N'THARUSHI.RATHNAYAKE@GMAIL.COM',1,N'AQAAAAEAAYagAAAAECu1AxTE/RqOb/7p/cqcjQHS3h/KBa1/5APbktW/SauihyR8nwooQI2Lt5OZ5+x31A==',N'7bd447ce-e5ba-4bff-935b-dcf736b6a27e',N'99666fa9-ea63-4bac-8875-4bf16efa2b5f',NULL,1,0,NULL,1,0),
(N'user-org-07',N'chamara.kumara.official@gmail.com',N'CHAMARA.KUMARA.OFFICIAL@GMAIL.COM',N'chamara.kumara.official@gmail.com',N'CHAMARA.KUMARA.OFFICIAL@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEF5NEd5MtXXs8E8WNz/9IeZw0K0KVelB+jZ13yaetX3GCAOfGm4Nb7ij6omzN4BTuA==',N'c90f0130-be47-4b9f-8d98-5d4b8d85c3c3',N'3a2c14e9-51d6-4da5-8c66-ae6ce09fdb69',NULL,1,0,NULL,1,0),
(N'user-org-08',N'kavindi.r99@gmail.com',N'KAVINDI.R99@GMAIL.COM',N'kavindi.r99@gmail.com',N'KAVINDI.R99@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEDZJLp2oajMJWdXpuVgk7Mqlg/yfR+Vle+c8ZmVwJbgg54O1FFj1fVWqUdTXhegPLg==',N'4024177d-2cc2-4426-a841-f41578c2166f',N'c3e97932-26c5-4e85-993b-4ca6c1fba38a',NULL,1,0,NULL,1,0),
(N'user-org-09',N'isuru.senanayake85@gmail.com',N'ISURU.SENANAYAKE85@GMAIL.COM',N'isuru.senanayake85@gmail.com',N'ISURU.SENANAYAKE85@GMAIL.COM',1,N'AQAAAAEAAYagAAAAENCMrECT6OpJ8kEtKX/tSFZRRfdAKQrJo92W0OmjNlRadvIU8aD4b8vsWY1vxjD/fA==',N'25c7a3d9-efca-49da-a9b3-8ca0b422b2f1',N'a2759f36-012c-4017-89f6-6a546336c6fc',NULL,1,0,NULL,1,0),
(N'user-org-10',N'madushani.eka@gmail.com',N'MADUSHANI.EKA@GMAIL.COM',N'madushani.eka@gmail.com',N'MADUSHANI.EKA@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEEAWLTH7Gq/tysccuq7deBR10xfoVqY4RTbmCMBkBxIV8fhFLjGwRxMZBYZlJGd9+w==',N'cd70dbce-f3b6-4111-9fed-f1e778d926bf',N'9179e692-6f6c-458b-95cf-e5248eae76a2',NULL,1,0,NULL,1,0),
(N'user-org-11',N'gayan.dissanayake.lk@gmail.com',N'GAYAN.DISSANAYAKE.LK@GMAIL.COM',N'gayan.dissanayake.lk@gmail.com',N'GAYAN.DISSANAYAKE.LK@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEBZE61wr9B/LhHLvZCzQYcaAzR6yXbE3QHsKGNTWqWaVWW1rnPUFcWpTQ45CgDi+NA==',N'b583eb59-0f72-4f09-9c33-2be7c94cfc5f',N'4f1df28b-7852-434f-9bd5-2ec4c796dc5d',NULL,1,0,NULL,1,0),
(N'user-org-12',N'oshadi.wijesinghe@gmail.com',N'OSHADI.WIJESINGHE@GMAIL.COM',N'oshadi.wijesinghe@gmail.com',N'OSHADI.WIJESINGHE@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEM//C1jyVeNZmezT7U9LFOFwftUvNrF39nnK+sDMkQqlpDmAPfVZ53pwkLV9khIktw==',N'1022b37f-bfbe-48ab-af0e-399d17078010',N'ff91907a-79be-426c-bf11-1393efb3cc73',NULL,1,0,NULL,1,0),
(N'user-org-13',N'nuwan.guna90@gmail.com',N'NUWAN.GUNA90@GMAIL.COM',N'nuwan.guna90@gmail.com',N'NUWAN.GUNA90@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEDxE05RICF/K/9ysyuValCFONT2sDyLwXhN0nu9AWA/+qMVfQ08s4OCLORCRlYjlzA==',N'db138c7b-e467-4024-ba5f-11fe18e2f85b',N'2714a5fe-531f-4b5f-84eb-8230b4d69ff6',NULL,1,0,NULL,1,0),
(N'user-org-14',N'hashini.peiris@gmail.com',N'HASHINI.PEIRIS@GMAIL.COM',N'hashini.peiris@gmail.com',N'HASHINI.PEIRIS@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEKRFZGxH/5fCTl97zN4PxVPcDNs4ISKipqHiDfJ4AmHsPR/Ek7xrIuZQy4t5LMoehw==',N'f67144b6-1079-4e73-964a-0ac344365587',N'f80037b1-6da2-41c2-af03-444106ae340f',NULL,1,0,NULL,1,0),
(N'user-org-15',N'sahan.liyanage22@gmail.com',N'SAHAN.LIYANAGE22@GMAIL.COM',N'sahan.liyanage22@gmail.com',N'SAHAN.LIYANAGE22@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEABySfL5X1G4wZpOBOG8YJRdB+9NBZsLoXg+m4ixC3/ecm886uGGL7ZVuKavY9LR7A==',N'6f675f93-0b2b-443f-97c5-d77cb2c17269',N'8f3dc50a-4960-4a39-bde4-4c9cfbe57a2f',NULL,1,0,NULL,1,0),
(N'user-org-16',N'thilini.muna@gmail.com',N'THILINI.MUNA@GMAIL.COM',N'thilini.muna@gmail.com',N'THILINI.MUNA@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEHTpntOiH8dhjkbxz5Czvmf+CElWYpcgch8gG9RS26LxdZVpUnmo9ohj7P2BMjPAFw==',N'32e67ea8-cb26-4bfd-9a7f-1f9d1d68d24e',N'94a7c162-3ceb-4b93-a604-30cb1c0a9e73',NULL,1,0,NULL,1,0),
(N'user-org-17',N'ruwan.weera77@gmail.com',N'RUWAN.WEERA77@GMAIL.COM',N'ruwan.weera77@gmail.com',N'RUWAN.WEERA77@GMAIL.COM',1,N'AQAAAAEAAYagAAAAENhPF4Hlkwnu/lwSTyyYCqwW2kPlFdFz0/c3ctMDSEcdb34sg4HKhv0tS2nBVEGJrQ==',N'1f250856-4f7a-49a5-afbb-18d3ba2f821a',N'56e0c49b-8700-43fb-bfd1-a1631b33c23b',NULL,1,0,NULL,1,0),
(N'user-org-18',N'dilini.karunaratne@gmail.com',N'DILINI.KARUNARATNE@GMAIL.COM',N'dilini.karunaratne@gmail.com',N'DILINI.KARUNARATNE@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEHm8hqhLHk7XU9qj9LYkfzxzXUDVVaTrO2IN3WmvaeQ0FXycvKG+AxsJJD3zGPTD5A==',N'73e1941e-85cd-4182-b976-b1c51758863d',N'd6fb7845-9dcc-49f0-85cf-43b38670fee9',NULL,1,0,NULL,1,0),
(N'user-org-19',N'asanka.samara@gmail.com',N'ASANKA.SAMARA@GMAIL.COM',N'asanka.samara@gmail.com',N'ASANKA.SAMARA@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEClz9UvK+KZpsxM1svYSOkwuzo8jfiyoTx88RVp9JsULNh86SnJF6jLH2heS4GRe3w==',N'1b6c7294-1d88-4356-ba76-03b5c682ca2f',N'da0b25b3-e8c3-47de-b458-1ae9aace3b49',NULL,1,0,NULL,1,0),
(N'user-org-20',N'nadeesha.wick94@gmail.com',N'NADEESHA.WICK94@GMAIL.COM',N'nadeesha.wick94@gmail.com',N'NADEESHA.WICK94@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEA6pr1X50F86ez9IKaaioIAe9YGR8USfgjJyG5uwNEjPAzpXPHnyI7L01E/pyABxrA==',N'5e0fa8fc-4518-4bba-88fb-2616441a3c03',N'0d6add04-7c48-422a-9e18-00bdc42589ae',NULL,1,0,NULL,1,0),
(N'user-vol-01',N'amila.tennakoon84@gmail.com',N'AMILA.TENNAKOON84@GMAIL.COM',N'amila.tennakoon84@gmail.com',N'AMILA.TENNAKOON84@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEMPoaDHBWD06ncUB7milFt9LsNdf0xp6Roe9EbuOUl532oAZ917lsFY6+2ojwW5U9w==',N'f051574c-6358-4f7b-82cd-8f6078113be1',N'08133076-e42d-4ada-9467-6a43a9c87a22',NULL,1,0,NULL,1,0),
(N'user-vol-02',N'nishanthi.silva.lk@gmail.com',N'NISHANTHI.SILVA.LK@GMAIL.COM',N'nishanthi.silva.lk@gmail.com',N'NISHANTHI.SILVA.LK@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEMRCOJvbtwgBv6dGIcWxE+lVRTQ0jMk4u5t59biYdcA0m03NZl8Mb7PsgYfMg95y5w==',N'02533f31-b6d4-4114-8d92-ba1ac4ce55f8',N'ebdc98ec-e666-4b7c-ab54-e18576c026c0',NULL,1,0,NULL,1,0),
(N'user-vol-03',N'pradeep.kumara91@gmail.com',N'PRADEEP.KUMARA91@GMAIL.COM',N'pradeep.kumara91@gmail.com',N'PRADEEP.KUMARA91@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEPoZ9TyhChj8qy3JZJOGIUOSaHq1IzzCPkQ1or9ScGKIgrgD9BTmfokkwaauVwIo/w==',N'65be77b3-c98f-4950-93ea-ccacf61bab79',N'fad1b7c1-76e1-4ec7-9ffe-3b8c8a7d503b',NULL,1,0,NULL,1,0),
(N'user-vol-04',N'sithumi.fernando01@gmail.com',N'SITHUMI.FERNANDO01@GMAIL.COM',N'sithumi.fernando01@gmail.com',N'SITHUMI.FERNANDO01@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEJs/+ZvH9SqgWR8tM19Pv2cKpb6wkaQ1vlE9jRdNn0r2hZJ4Dj91ITxudC+3skYp4Q==',N'53710a68-ff75-4bf2-8e36-7abca822dfd5',N'a7c71a3c-302a-4093-a3c8-c53732c82250',NULL,1,0,NULL,1,0),
(N'user-vol-05',N'chathurap.93@gmail.com',N'CHATHURAP.93@GMAIL.COM',N'chathurap.93@gmail.com',N'CHATHURAP.93@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEF5lkpSiJS2hngQd+KBGPhB3TK6DZin70sVeviDwvfNUjVjInHPwXW73c/Vrw3V/vA==',N'd5d23119-653b-473d-a3c8-3b6adc0e553c',N'f03d8b68-d4c1-4872-89c2-5b0db3f59480',NULL,1,0,NULL,1,0),
(N'user-vol-06',N'lakmini.dissa@gmail.com',N'LAKMINI.DISSA@GMAIL.COM',N'lakmini.dissa@gmail.com',N'LAKMINI.DISSA@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEMc5rwSDV3/pEizxeSxhwku6sPlp0u6ZxOj8Llj0os2szrfcjLE+p2vkkOlVMoRf/g==',N'be345dbc-a58f-4ac5-8a3f-a6c1a6c1185e',N'5ad15dbd-7d59-486b-a0d4-ad53e87a09ed',NULL,1,0,NULL,1,0),
(N'user-vol-07',N'dinuka.jaya88@gmail.com',N'DINUKA.JAYA88@GMAIL.COM',N'dinuka.jaya88@gmail.com',N'DINUKA.JAYA88@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEAOUJLAW2BnQe15edpCzROfMM6ssZ4zEUFKnaxkANoPpCnFK7z6qEBh1UHob4EcERA==',N'081a9cc9-d3ec-4f50-ba81-9273ced548ba',N'6565b99b-64c1-4240-8b55-1c48cd502f19',NULL,1,0,NULL,1,0),
(N'user-vol-08',N'nimesha.rana@gmail.com',N'NIMESHA.RANA@GMAIL.COM',N'nimesha.rana@gmail.com',N'NIMESHA.RANA@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEKcme31PnlDs/edjG9HUp2rcr0w4KFfFbmrHrZKsaU+VH5TyEuFqz2EQFZLeSLZWdg==',N'6a9c8da0-3585-406b-ad8a-ae5ddda0841c',N'8064b1f0-df40-4905-874a-adf81513c0d0',NULL,1,0,NULL,1,0),
(N'user-vol-09',N'roshan.weerakoon79@gmail.com',N'ROSHAN.WEERAKOON79@GMAIL.COM',N'roshan.weerakoon79@gmail.com',N'ROSHAN.WEERAKOON79@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEN0j+bNRFQcw8TKzum6Xy4KTlnCrYCfsHIXoN+/O5Ilu4gbrNuATQaqhftuqqdnF1w==',N'70a4a325-783e-4bff-bd02-d860bd012656',N'08c2bd4e-0aef-43ed-96db-c9d5dc1d90e6',NULL,1,0,NULL,1,0),
(N'user-vol-10',N'shashika.senaratne@gmail.com',N'SHASHIKA.SENARATNE@GMAIL.COM',N'shashika.senaratne@gmail.com',N'SHASHIKA.SENARATNE@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEAOe+f8IyNChnQ7CpvNP9V5RXi6TO0JUH3vEHqruByfQBPu8Pyz/yME85HETkP3I9Q==',N'747d744b-2adf-4200-8996-b8c41612f052',N'fc2dd828-628b-46d4-81c4-42ce3e4494da',NULL,1,0,NULL,1,0),
(N'user-vol-11',N'supun.liyanage96@gmail.com',N'SUPUN.LIYANAGE96@GMAIL.COM',N'supun.liyanage96@gmail.com',N'SUPUN.LIYANAGE96@GMAIL.COM',1,N'AQAAAAEAAYagAAAAENASIn0kXSdoe6z7v10gFnr5ZGvoEwMRuHsr4o51RPd6+vyAblNfN2TYtLjCNqEhxg==',N'a51ecfd6-9131-4040-8c02-3776ef711a12',N'526975c2-74f3-4000-9a95-fe141172dfcf',NULL,1,0,NULL,1,0),
(N'user-vol-12',N'ishara.rajapaksa.lk@gmail.com',N'ISHARA.RAJAPAKSA.LK@GMAIL.COM',N'ishara.rajapaksa.lk@gmail.com',N'ISHARA.RAJAPAKSA.LK@GMAIL.COM',1,N'AQAAAAEAAYagAAAAENlnxl4zZ3D6p1eeKpqlwcapWvJHFvHjJSH+SqaNlLfIj/632NubQq3ApqJm+G7T8w==',N'307b348a-97b6-4ebf-a4f8-e83ac23298b3',N'466af41a-6df5-4e81-8f75-69a4e0388f20',NULL,1,0,NULL,1,0),
(N'user-vol-13',N'kamal.deshapriya@gmail.com',N'KAMAL.DESHAPRIYA@GMAIL.COM',N'kamal.deshapriya@gmail.com',N'KAMAL.DESHAPRIYA@GMAIL.COM',1,N'AQAAAAEAAYagAAAAENvukdsv0xaTQI6VoVZw679xuFjYIAguCTXODPsKn77b3P9Bgia4IjhdPL5wuDc9cw==',N'942eef5c-a26c-4ca2-92ad-6ba3c50dec19',N'8f08ed38-c95c-4a31-9d49-8c0deb9e51fa',NULL,1,0,NULL,1,0),
(N'user-vol-14',N'anjalika.peiris94@gmail.com',N'ANJALIKA.PEIRIS94@GMAIL.COM',N'anjalika.peiris94@gmail.com',N'ANJALIKA.PEIRIS94@GMAIL.COM',1,N'AQAAAAEAAYagAAAAELESm9KkBvjDTR5Tg2SdWBytDoZDXqYzX0rKFwCkSv29DfntNoqnyE2IgEJabgeHGw==',N'653ae6a1-c119-4c84-9049-04c1463e816a',N'60fbb17f-2d45-458b-b9fb-7319743a1e39',NULL,1,0,NULL,1,0),
(N'user-vol-15',N'mahesh.bandara82@gmail.com',N'MAHESH.BANDARA82@GMAIL.COM',N'mahesh.bandara82@gmail.com',N'MAHESH.BANDARA82@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEN1d7u8muXksW8IbXOxMV+mqhnGccyu3SUyrq5zALeeODkXy5e585BGBz3JosicrAQ==',N'4bbf69ae-5bfd-49e0-8f07-1b1290a84b80',N'a0a7162a-1b77-4357-b119-efcf957010ad',NULL,1,0,NULL,1,0),
(N'user-vol-16',N'hansini.guna99@gmail.com',N'HANSINI.GUNA99@GMAIL.COM',N'hansini.guna99@gmail.com',N'HANSINI.GUNA99@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEJOBprA22GE97e20SJIQTVZTpRsUP7ef8oilbhlUaF9ezdZviWM0GPK/ixWoyMliMQ==',N'0b0d5f83-1c2f-4a2d-827a-15387999242f',N'01d48fe3-5987-4f2f-a9eb-9fa488877ee6',NULL,1,0,NULL,1,0),
(N'user-vol-17',N'janaka.samara.lk@gmail.com',N'JANAKA.SAMARA.LK@GMAIL.COM',N'janaka.samara.lk@gmail.com',N'JANAKA.SAMARA.LK@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEJNs57AUwQ4kkDvG6kaVzRgK3zlLU9NVh6K4v9i0Z/reqczdrW/mUK1YEI0YBYZ24w==',N'a1444010-2a99-4439-9187-e7f133cc2859',N'2450e839-5f63-4a2e-8e15-fb3fa1a6e574',NULL,1,0,NULL,1,0),
(N'user-vol-18',N'piyumi.wijeratne@gmail.com',N'PIYUMI.WIJERATNE@GMAIL.COM',N'piyumi.wijeratne@gmail.com',N'PIYUMI.WIJERATNE@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEPxl4zCHsMnXntwDu6uZqlXPLIe8V8C+k2x2KfhoFid9Z0wlTThcbTh5mTrrtMV6EQ==',N'b6168e28-0690-488d-8d36-f99820106174',N'ffe42d68-5a35-43af-91a9-2c35501405f2',NULL,1,0,NULL,1,0),
(N'user-vol-19',N'sudeepa.herath95@gmail.com',N'SUDEEPA.HERATH95@GMAIL.COM',N'sudeepa.herath95@gmail.com',N'SUDEEPA.HERATH95@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEMJ0IKfZuwSvoFTst3zd093rJ+2JwwYtOCTApYTzdlr/mPoF4k2VeOleSZ0A6SaHzw==',N'15eada97-ee15-4b5b-a799-e04da3e05bc2',N'e65a9bc7-8d47-4f6a-b11b-8c32b350ea09',NULL,1,0,NULL,1,0),
(N'user-vol-20',N'malithi.kalu@gmail.com',N'MALITHI.KALU@GMAIL.COM',N'malithi.kalu@gmail.com',N'MALITHI.KALU@GMAIL.COM',1,N'AQAAAAEAAYagAAAAEAppbvAOPdY26nrYASxSG1slmfiS2vfz+Sle+7nvmNaLVkMhetUc93PfUu+a2UfPYA==',N'a632fbae-c265-4a07-99ef-4f3fb88c9ad1',N'bccec8d5-f40e-40f4-9277-0758254b9c74',NULL,1,0,DATEADD(year,10,SYSDATETIMEOFFSET()),1,0);

INSERT INTO [AspNetUserRoles] ([UserId],[RoleId]) VALUES
(N'user-admin',N'role-admin'),
(N'user-org-01',N'role-organizer'),
(N'user-org-02',N'role-organizer'),
(N'user-org-03',N'role-organizer'),
(N'user-org-04',N'role-organizer'),
(N'user-org-05',N'role-organizer'),
(N'user-org-06',N'role-organizer'),
(N'user-org-07',N'role-organizer'),
(N'user-org-08',N'role-organizer'),
(N'user-org-09',N'role-organizer'),
(N'user-org-10',N'role-organizer'),
(N'user-org-11',N'role-organizer'),
(N'user-org-12',N'role-organizer'),
(N'user-org-13',N'role-organizer'),
(N'user-org-14',N'role-organizer'),
(N'user-org-15',N'role-organizer'),
(N'user-org-16',N'role-organizer'),
(N'user-org-17',N'role-organizer'),
(N'user-org-18',N'role-organizer'),
(N'user-org-19',N'role-organizer'),
(N'user-org-20',N'role-organizer'),
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
(N'user-vol-12',N'role-volunteer'),
(N'user-vol-13',N'role-volunteer'),
(N'user-vol-14',N'role-volunteer'),
(N'user-vol-15',N'role-volunteer'),
(N'user-vol-16',N'role-volunteer'),
(N'user-vol-17',N'role-volunteer'),
(N'user-vol-18',N'role-volunteer'),
(N'user-vol-19',N'role-volunteer'),
(N'user-vol-20',N'role-volunteer');

SET IDENTITY_INSERT [UserProfiles] ON;

INSERT INTO [UserProfiles] ([Id],[UserId],[FullName],[RoleName],[PublicEmail],[ContactNumber],[Bio],[Skills],[Availability],[ProfileImageUrl],[OrganizationName],[IsVerified],[CreatedAt]) VALUES
(1,N'user-admin',N'System Administrator',N'Admin',N'admin@gmail.com',N'078-9815572',N'System administrator account responsible for managing users, verifying organizers, handling violations, and supporting the final OCVMS project demonstration.',N'System administration, user verification, role management, account control, event monitoring',N'Available for system monitoring, user verification, and project demonstration',N'/uploads/profiles/profile_admin.png',N'OCVMS Administration',1,DATEADD(day,-32,GETUTCDATE())),
(2,N'user-org-01',N'Kasun Perera',N'Organizer',N'kasun.perera92@gmail.com',N'0715048663',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_01.png',N'Gonulla Youth',1,DATEADD(day,-3,GETUTCDATE())),
(3,N'user-org-02',N'Nimali Fernando',N'Organizer',N'nimali.fernando88@gmail.com',N'0724403548',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_02.png',N'Gonulla Youth',1,DATEADD(day,-34,GETUTCDATE())),
(4,N'user-org-03',N'Dasun Silva',N'Organizer',N'dasunsilva.lk@gmail.com',N'0793743274',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_03.png',N'Gonulla Youth',1,DATEADD(day,-34,GETUTCDATE())),
(5,N'user-org-04',N'Sanduni Jayasooriya',N'Organizer',N'sanduni.jaya@gmail.com',N'0736032416',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_04.png',N'Gonulla Youth',1,DATEADD(day,-1,GETUTCDATE())),
(6,N'user-org-05',N'Lahiru Bandara',N'Organizer',N'lahiru.bandara95@gmail.com',N'0797360744',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_05.png',N'Gonulla Youth',1,DATEADD(day,-18,GETUTCDATE())),
(7,N'user-org-06',N'Tharushi Rathnayake',N'Organizer',N'tharushi.rathnayake@gmail.com',N'0747968917',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_06.png',N'National School Pannala',1,DATEADD(day,-29,GETUTCDATE())),
(8,N'user-org-07',N'Chamara Kumara',N'Organizer',N'chamara.kumara.official@gmail.com',N'0742407816',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_07.png',N'Gonulla Youth',1,DATEADD(day,-34,GETUTCDATE())),
(9,N'user-org-08',N'Kavindi Rajapaksha',N'Organizer',N'kavindi.r99@gmail.com',N'0771327046',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_08.png',N'Gonulla Youth',1,DATEADD(day,-1,GETUTCDATE())),
(10,N'user-org-09',N'Isuru Senanayake',N'Organizer',N'isuru.senanayake85@gmail.com',N'0751160176',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_09.png',N'Gonulla Youth',1,DATEADD(day,-17,GETUTCDATE())),
(11,N'user-org-10',N'Madushani Ekanayake',N'Organizer',N'madushani.eka@gmail.com',N'0784778098',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_10.png',N'Sandalanka Central Collage',1,DATEADD(day,-30,GETUTCDATE())),
(12,N'user-org-11',N'Gayan Dissanayake',N'Organizer',N'gayan.dissanayake.lk@gmail.com',N'0792613826',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_11.png',N'Gonulla Youth',1,DATEADD(day,-4,GETUTCDATE())),
(13,N'user-org-12',N'Oshadi Wijesinghe',N'Organizer',N'oshadi.wijesinghe@gmail.com',N'0764539902',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_12.png',N'Gonulla Youth',1,DATEADD(day,-27,GETUTCDATE())),
(14,N'user-org-13',N'Nuwan Gunawardena',N'Organizer',N'nuwan.guna90@gmail.com',N'0765777756',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_13.png',N'Gonulla Youth',1,DATEADD(day,-17,GETUTCDATE())),
(15,N'user-org-14',N'Hashini Peiris',N'Organizer',N'hashini.peiris@gmail.com',N'0793264771',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_14.png',N'Gonulla Youth',1,DATEADD(day,-32,GETUTCDATE())),
(16,N'user-org-15',N'Sahan Liyanage',N'Organizer',N'sahan.liyanage22@gmail.com',N'0746222989',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_15.png',N'New Farmers of the North West',1,DATEADD(day,-36,GETUTCDATE())),
(17,N'user-org-16',N'Thilini Munasinghe',N'Organizer',N'thilini.muna@gmail.com',N'0774712171',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_16.png',N'Gonulla Youth',1,DATEADD(day,-3,GETUTCDATE())),
(18,N'user-org-17',N'Ruwan Weerasinghe',N'Organizer',N'ruwan.weera77@gmail.com',N'0762733429',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_17.png',N'Gonulla Youth',1,DATEADD(day,-28,GETUTCDATE())),
(19,N'user-org-18',N'Dilini Karunaratne',N'Organizer',N'dilini.karunaratne@gmail.com',N'0733682666',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_18.png',N'Gonulla Youth',1,DATEADD(day,-38,GETUTCDATE())),
(20,N'user-org-19',N'Asanka Samarawickrama',N'Organizer',N'asanka.samara@gmail.com',N'0777733266',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_19.png',N'Gonulla Youth',1,DATEADD(day,-19,GETUTCDATE())),
(21,N'user-org-20',N'Nadeesha Wickramasinghe',N'Organizer',N'nadeesha.wick94@gmail.com',N'0743566180',N'Organizer account responsible for creating community events, managing volunteers, and coordinating organization-based activities.',N'Event planning, volunteer coordination, communication, community service',N'Available during weekends and scheduled event days',N'/uploads/profiles/profile_org_20.png',N'Gonulla Youth',1,DATEADD(day,-17,GETUTCDATE())),
(22,N'user-vol-01',N'Amila Tennakoon',N'Volunteer',N'amila.tennakoon84@gmail.com',N'0764439321',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_01.png',NULL,1,DATEADD(day,-23,GETUTCDATE())),
(23,N'user-vol-02',N'Nishanthi Silva',N'Volunteer',N'nishanthi.silva.lk@gmail.com',N'0790189803',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_02.png',NULL,1,DATEADD(day,-7,GETUTCDATE())),
(24,N'user-vol-03',N'Pradeep Kumara',N'Volunteer',N'pradeep.kumara91@gmail.com',N'0796623557',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_03.png',NULL,1,DATEADD(day,-21,GETUTCDATE())),
(25,N'user-vol-04',N'Sithumi Fernando',N'Volunteer',N'sithumi.fernando01@gmail.com',N'0785178128',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_04.png',NULL,1,DATEADD(day,-29,GETUTCDATE())),
(26,N'user-vol-05',N'Chathura Perera',N'Volunteer',N'chathurap.93@gmail.com',N'0734850536',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_05.png',NULL,1,DATEADD(day,-2,GETUTCDATE())),
(27,N'user-vol-06',N'Lakmini Dissanayake',N'Volunteer',N'lakmini.dissa@gmail.com',N'0786419308',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_06.png',NULL,1,DATEADD(day,-20,GETUTCDATE())),
(28,N'user-vol-07',N'Dinuka Jayawardena',N'Volunteer',N'dinuka.jaya88@gmail.com',N'0712047534',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_07.png',NULL,1,DATEADD(day,-25,GETUTCDATE())),
(29,N'user-vol-08',N'Nimesha Ranasinghe',N'Volunteer',N'nimesha.rana@gmail.com',N'0741115675',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_08.png',NULL,1,DATEADD(day,-16,GETUTCDATE())),
(30,N'user-vol-09',N'Roshan Weerakoon',N'Volunteer',N'roshan.weerakoon79@gmail.com',N'0716645756',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_09.png',NULL,1,DATEADD(day,-11,GETUTCDATE())),
(31,N'user-vol-10',N'Shashika Senaratne',N'Volunteer',N'shashika.senaratne@gmail.com',N'0784695294',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_10.png',NULL,1,DATEADD(day,-9,GETUTCDATE())),
(32,N'user-vol-11',N'Supun Liyanage',N'Volunteer',N'supun.liyanage96@gmail.com',N'0765310012',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_11.png',NULL,1,DATEADD(day,-9,GETUTCDATE())),
(33,N'user-vol-12',N'Ishara Rajapaksa',N'Volunteer',N'ishara.rajapaksa.lk@gmail.com',N'0737587162',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_12.png',NULL,1,DATEADD(day,-8,GETUTCDATE())),
(34,N'user-vol-13',N'Kamal Deshapriya',N'Volunteer',N'kamal.deshapriya@gmail.com',N'0738879723',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_13.png',NULL,1,DATEADD(day,-25,GETUTCDATE())),
(35,N'user-vol-14',N'Anjalika Peiris',N'Volunteer',N'anjalika.peiris94@gmail.com',N'0733742021',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_14.png',NULL,1,DATEADD(day,-23,GETUTCDATE())),
(36,N'user-vol-15',N'Mahesh Bandara',N'Volunteer',N'mahesh.bandara82@gmail.com',N'0762199856',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_15.png',NULL,1,DATEADD(day,-21,GETUTCDATE())),
(37,N'user-vol-16',N'Hansini Gunasekara',N'Volunteer',N'hansini.guna99@gmail.com',N'0760604113',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_16.png',NULL,1,DATEADD(day,-15,GETUTCDATE())),
(38,N'user-vol-17',N'Janaka Samarasinghe',N'Volunteer',N'janaka.samara.lk@gmail.com',N'0778343224',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_17.png',NULL,1,DATEADD(day,-38,GETUTCDATE())),
(39,N'user-vol-18',N'Piyumi Wijeratne',N'Volunteer',N'piyumi.wijeratne@gmail.com',N'0724278248',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_18.png',NULL,1,DATEADD(day,-24,GETUTCDATE())),
(40,N'user-vol-19',N'Sudeepa Herath',N'Volunteer',N'sudeepa.herath95@gmail.com',N'0745839647',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_19.png',NULL,1,DATEADD(day,-23,GETUTCDATE())),
(41,N'user-vol-20',N'Malithi Kaluarachchi',N'Volunteer',N'malithi.kalu@gmail.com',N'0751879316',N'Volunteer account available to join events, support community projects, share updates, and request help when needed.',N'Teamwork, communication, field support, community service',N'Available on weekends and after working hours',N'/uploads/profiles/profile_vol_20.png',NULL,1,DATEADD(day,-19,GETUTCDATE()));

SET IDENTITY_INSERT [UserProfiles] OFF;

SET IDENTITY_INSERT [VolunteerEvents] ON;

INSERT INTO [VolunteerEvents] ([Id],[Title],[Description],[Location],[EventDate],[Capacity],[Status],[ImageUrl],[OrganizerProfileId],[CreatedAt],[EventTime],[RegistrationOpenDate],[RegistrationClosingDate]) VALUES
(1,N'Beach Cleanup Drive',N'Help us remove plastic and debris from the shoreline. Wear comfortable clothes.',N'Mount Lavinia Beach',CAST(N'2026-05-15' AS date),50,N'Upcoming',N'/uploads/events/event_01.png',2,DATEADD(day,-3,GETUTCDATE()),CAST(N'08:00:00' AS time),CAST(N'2026-04-28' AS date),CAST(N'2026-05-14' AS date)),
(2,N'Elderly Home Visit',N'Spending time with elders, sharing meals, and organizing small entertainment activities.',N'Grace Care Center',CAST(N'2026-05-20' AS date),15,N'Upcoming',N'/uploads/events/event_02.png',3,DATEADD(day,-13,GETUTCDATE()),CAST(N'09:30:00' AS time),CAST(N'2026-05-03' AS date),CAST(N'2026-05-19' AS date)),
(3,N'Tree Planting Project',N'Join us for World Environment Day to plant 100 native trees. Shovels provided.',N'Viharamahadevi Park',CAST(N'2026-06-05' AS date),30,N'Upcoming',N'/uploads/events/event_03.png',4,DATEADD(day,-17,GETUTCDATE()),CAST(N'07:00:00' AS time),CAST(N'2026-05-19' AS date),CAST(N'2026-06-04' AS date)),
(4,N'Blood Donation Camp',N'Annual blood donation drive. Please bring a valid ID and stay hydrated.',N'Community Hall, Colombo',CAST(N'2026-06-12' AS date),100,N'Upcoming',N'/uploads/events/event_04.png',5,DATEADD(day,-18,GETUTCDATE()),CAST(N'10:00:00' AS time),CAST(N'2026-05-26' AS date),CAST(N'2026-06-11' AS date)),
(5,N'Teaching Kids Math',N'Tutoring primary school children. No teaching experience required, just patience!',N'Hope Orphanage',CAST(N'2026-06-18' AS date),10,N'Upcoming',N'/uploads/events/event_05.png',6,DATEADD(day,-19,GETUTCDATE()),CAST(N'14:00:00' AS time),CAST(N'2026-06-01' AS date),CAST(N'2026-06-17' AS date)),
(6,N'Stray Dog Feeding',N'Providing food and water to stray animals in the area. Volunteers bring dry food.',N'Fort Railway Station',CAST(N'2026-06-22' AS date),8,N'Upcoming',N'/uploads/events/event_06.png',7,DATEADD(day,-16,GETUTCDATE()),CAST(N'17:00:00' AS time),CAST(N'2026-06-05' AS date),CAST(N'2026-06-21' AS date)),
(7,N'Coding for Beginners',N'A free workshop teaching HTML/CSS basics to local youth. Bring your laptop.',N'Public Library Lab',CAST(N'2026-07-02' AS date),20,N'Upcoming',N'/uploads/events/event_07.png',8,DATEADD(day,-10,GETUTCDATE()),CAST(N'18:00:00' AS time),CAST(N'2026-06-15' AS date),CAST(N'2026-07-01' AS date)),
(8,N'Library Book Sorting',N'Organizing the newly donated books and cleaning the shelves.',N'Municipal Library',CAST(N'2026-07-08' AS date),12,N'Upcoming',N'/uploads/events/event_08.png',9,DATEADD(day,-14,GETUTCDATE()),CAST(N'09:00:00' AS time),CAST(N'2026-06-21' AS date),CAST(N'2026-07-07' AS date)),
(9,N'Community Kitchen',N'Helping prepare and serve lunch for homeless individuals in the city.',N'St. Peter''s Church',CAST(N'2026-07-15' AS date),25,N'Upcoming',N'/uploads/events/event_09.png',10,DATEADD(day,-7,GETUTCDATE()),CAST(N'11:30:00' AS time),CAST(N'2026-06-28' AS date),CAST(N'2026-07-14' AS date)),
(10,N'River Bank Restoration',N'Clearing invasive plants and cleaning the riverbank area.',N'Kelani River Bank',CAST(N'2026-07-28' AS date),40,N'Upcoming',N'/uploads/events/event_10.png',11,DATEADD(day,-11,GETUTCDATE()),CAST(N'08:00:00' AS time),CAST(N'2026-07-11' AS date),CAST(N'2026-07-27' AS date)),
(11,N'English Speaking Club',N'Conversational English practice for school leavers through fun games.',N'Youth Center, Kandy',CAST(N'2026-08-04' AS date),15,N'Upcoming',N'/uploads/events/event_11.png',12,DATEADD(day,-3,GETUTCDATE()),CAST(N'16:00:00' AS time),CAST(N'2026-07-18' AS date),CAST(N'2026-08-03' AS date)),
(12,N'Free Health Clinic',N'Assisting doctors with registration and crowd control during the free clinic.',N'Rural Health Center',CAST(N'2026-08-12' AS date),60,N'Upcoming',N'/uploads/events/event_12.png',13,DATEADD(day,-2,GETUTCDATE()),CAST(N'08:30:00' AS time),CAST(N'2026-07-26' AS date),CAST(N'2026-08-11' AS date)),
(13,N'Recycling Awareness',N'Distributing flyers and explaining waste segregation to the public.',N'City Square Mall',CAST(N'2026-08-20' AS date),20,N'Upcoming',N'/uploads/events/event_13.png',14,DATEADD(day,-11,GETUTCDATE()),CAST(N'10:00:00' AS time),CAST(N'2026-08-03' AS date),CAST(N'2026-08-19' AS date)),
(14,N'Park Bench Painting',N'repainting old benches and fences. Wear clothes that you don''t mind getting stained.',N'Lakeside Park',CAST(N'2026-08-25' AS date),10,N'Upcoming',N'/uploads/events/event_14.png',15,DATEADD(day,-14,GETUTCDATE()),CAST(N'15:00:00' AS time),CAST(N'2026-08-08' AS date),CAST(N'2026-08-24' AS date)),
(15,N'Music Therapy Session',N'Volunteers with musical skills needed to play for kids in the ward.',N'Children’s Hospital',CAST(N'2026-09-05' AS date),5,N'Upcoming',N'/uploads/events/event_15.png',16,DATEADD(day,-17,GETUTCDATE()),CAST(N'16:30:00' AS time),CAST(N'2026-08-19' AS date),CAST(N'2026-09-04' AS date)),
(16,N'Flood Relief Packing',N'Sorting and packing dry rations and clothes for flood victims.',N'Disaster Relief HQ',CAST(N'2026-09-10' AS date),45,N'Upcoming',N'/uploads/events/event_16.png',17,DATEADD(day,-19,GETUTCDATE()),CAST(N'09:00:00' AS time),CAST(N'2026-08-24' AS date),CAST(N'2026-09-09' AS date)),
(17,N'Youth Career Seminar',N'Assisting in organizing a career guidance seminar for IT students.',N'University Auditorium',CAST(N'2026-09-18' AS date),80,N'Upcoming',N'/uploads/events/event_17.png',18,DATEADD(day,-20,GETUTCDATE()),CAST(N'13:00:00' AS time),CAST(N'2026-09-01' AS date),CAST(N'2026-09-17' AS date)),
(18,N'City Marathon Guard',N'Directing runners and distributing water bottles at various stations.',N'Galle Face Green',CAST(N'2026-09-25' AS date),100,N'Upcoming',N'/uploads/events/event_18.png',19,DATEADD(day,-2,GETUTCDATE()),CAST(N'05:30:00' AS time),CAST(N'2026-09-08' AS date),CAST(N'2026-09-24' AS date)),
(19,N'Organic Farming Workshop',N'Learn and help with organic composting and planting seasonal vegetables.',N'Agri-Tech Farm',CAST(N'2026-10-05' AS date),25,N'Upcoming',N'/uploads/events/event_19.png',20,DATEADD(day,-15,GETUTCDATE()),CAST(N'08:00:00' AS time),CAST(N'2026-09-18' AS date),CAST(N'2026-10-04' AS date)),
(20,N'Senior Tech Support',N'Helping senior citizens learn how to use smartphones and WhatsApp.',N'Community Center',CAST(N'2026-10-12' AS date),10,N'Upcoming',N'/uploads/events/event_20.png',21,DATEADD(day,-16,GETUTCDATE()),CAST(N'14:00:00' AS time),CAST(N'2026-09-25' AS date),CAST(N'2026-10-11' AS date)),
(21,N'School Supplies Drive',N'Sorting and packing stationery and bags for underprivileged students.',N'City Education Office',CAST(N'2026-10-20' AS date),20,N'Upcoming',N'/uploads/events/event_21.png',2,DATEADD(day,-15,GETUTCDATE()),CAST(N'09:00:00' AS time),CAST(N'2026-10-03' AS date),CAST(N'2026-10-19' AS date)),
(22,N'Street Light Mapping',N'Walking through streets to identify and report broken street lights.',N'Suburban Neighborhood',CAST(N'2026-10-28' AS date),12,N'Upcoming',N'/uploads/events/event_22.png',3,DATEADD(day,-13,GETUTCDATE()),CAST(N'18:30:00' AS time),CAST(N'2026-10-11' AS date),CAST(N'2026-10-27' AS date)),
(23,N'Yoga for Wellbeing',N'Assisting the instructor in setting up mats and registering participants.',N'Public Gardens',CAST(N'2026-11-05' AS date),40,N'Upcoming',N'/uploads/events/event_23.png',4,DATEADD(day,-1,GETUTCDATE()),CAST(N'06:00:00' AS time),CAST(N'2026-10-19' AS date),CAST(N'2026-11-04' AS date)),
(24,N'Art Therapy Workshop',N'Helping patients express themselves through painting and crafts.',N'Mental Health Center',CAST(N'2026-11-12' AS date),15,N'Upcoming',N'/uploads/events/event_24.png',5,DATEADD(day,-1,GETUTCDATE()),CAST(N'14:00:00' AS time),CAST(N'2026-10-26' AS date),CAST(N'2026-11-11' AS date)),
(25,N'Old Laptop Repairs',N'Refurbishing donated laptops to be given to village schools.',N'Tech Hub Garage',CAST(N'2026-11-18' AS date),8,N'Upcoming',N'/uploads/events/event_25.png',6,DATEADD(day,-4,GETUTCDATE()),CAST(N'10:00:00' AS time),CAST(N'2026-11-01' AS date),CAST(N'2026-11-17' AS date)),
(26,N'Winter Clothing Drive',N'Collecting and folding warm clothes for distribution to cold areas.',N'Red Cross Building',CAST(N'2026-11-25' AS date),30,N'Upcoming',N'/uploads/events/event_26.png',7,DATEADD(day,-9,GETUTCDATE()),CAST(N'08:30:00' AS time),CAST(N'2026-11-08' AS date),CAST(N'2026-11-24' AS date)),
(27,N'Holiday Meal Prep',N'Preparing special festive meals for low-income families in the area.',N'Salvation Army Hall',CAST(N'2026-12-10' AS date),20,N'Upcoming',N'/uploads/events/event_27.png',8,DATEADD(day,-2,GETUTCDATE()),CAST(N'11:00:00' AS time),CAST(N'2026-11-23' AS date),CAST(N'2026-12-09' AS date)),
(28,N'Beach Safety Patrol',N'Assisting lifeguards in advising tourists about safe swimming areas.',N'Coastal Tourist Zone',CAST(N'2026-12-15' AS date),10,N'Upcoming',N'/uploads/events/event_28.png',9,DATEADD(day,-4,GETUTCDATE()),CAST(N'10:00:00' AS time),CAST(N'2026-11-28' AS date),CAST(N'2026-12-14' AS date)),
(29,N'Blind Walk Guide',N'Being a sighted guide for visually impaired runners in a 5km walk.',N'Town Running Track',CAST(N'2026-12-20' AS date),25,N'Upcoming',N'/uploads/events/event_29.png',10,DATEADD(day,-8,GETUTCDATE()),CAST(N'16:00:00' AS time),CAST(N'2026-12-03' AS date),CAST(N'2026-12-19' AS date)),
(30,N'New Year Tree Recycling',N'Helping to mulch discarded trees for use in public garden composting.',N'Municipal Waste Yard',CAST(N'2027-01-05' AS date),15,N'Upcoming',N'/uploads/events/event_30.png',11,DATEADD(day,-12,GETUTCDATE()),CAST(N'09:00:00' AS time),CAST(N'2026-12-19' AS date),CAST(N'2027-01-04' AS date));

SET IDENTITY_INSERT [VolunteerEvents] OFF;

SET IDENTITY_INSERT [EventRegistrations] ON;

INSERT INTO [EventRegistrations] ([Id],[VolunteerEventId],[UserId],[RegistrationDate],[CreatedAt]) VALUES
(1,1,N'user-vol-01',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(2,1,N'user-vol-02',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(3,1,N'user-vol-03',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(4,1,N'user-vol-04',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(5,1,N'user-vol-05',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(6,2,N'user-vol-02',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(7,2,N'user-vol-03',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(8,2,N'user-vol-04',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(9,3,N'user-vol-03',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(10,3,N'user-vol-04',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(11,3,N'user-vol-05',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(12,4,N'user-vol-04',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(13,4,N'user-vol-05',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(14,4,N'user-vol-06',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(15,4,N'user-vol-07',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(16,4,N'user-vol-08',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(17,4,N'user-vol-09',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(18,4,N'user-vol-10',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(19,4,N'user-vol-11',DATEADD(day,-8,GETDATE()),DATEADD(day,-8,GETUTCDATE())),
(20,5,N'user-vol-05',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(21,5,N'user-vol-06',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(22,5,N'user-vol-07',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(23,6,N'user-vol-06',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(24,6,N'user-vol-07',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(25,6,N'user-vol-08',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(26,7,N'user-vol-07',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(27,7,N'user-vol-08',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(28,7,N'user-vol-09',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(29,8,N'user-vol-08',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(30,8,N'user-vol-09',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(31,8,N'user-vol-10',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(32,9,N'user-vol-09',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(33,9,N'user-vol-10',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(34,9,N'user-vol-11',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(35,10,N'user-vol-10',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(36,10,N'user-vol-11',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(37,10,N'user-vol-12',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(38,10,N'user-vol-13',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(39,11,N'user-vol-11',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(40,11,N'user-vol-12',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(41,11,N'user-vol-13',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(42,12,N'user-vol-12',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(43,12,N'user-vol-13',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(44,12,N'user-vol-14',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(45,12,N'user-vol-15',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(46,12,N'user-vol-16',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(47,12,N'user-vol-17',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(48,13,N'user-vol-13',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(49,13,N'user-vol-14',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(50,13,N'user-vol-15',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(51,14,N'user-vol-14',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(52,14,N'user-vol-15',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(53,14,N'user-vol-16',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(54,15,N'user-vol-15',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(55,15,N'user-vol-16',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(56,15,N'user-vol-17',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(57,16,N'user-vol-16',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(58,16,N'user-vol-17',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(59,16,N'user-vol-18',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(60,16,N'user-vol-19',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(61,17,N'user-vol-17',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(62,17,N'user-vol-18',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(63,17,N'user-vol-19',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(64,17,N'user-vol-20',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(65,17,N'user-vol-01',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(66,17,N'user-vol-02',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(67,17,N'user-vol-03',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(68,17,N'user-vol-04',DATEADD(day,-8,GETDATE()),DATEADD(day,-8,GETUTCDATE())),
(69,18,N'user-vol-18',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(70,18,N'user-vol-19',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(71,18,N'user-vol-20',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(72,18,N'user-vol-01',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(73,18,N'user-vol-02',DATEADD(day,-5,GETDATE()),DATEADD(day,-5,GETUTCDATE())),
(74,18,N'user-vol-03',DATEADD(day,-6,GETDATE()),DATEADD(day,-6,GETUTCDATE())),
(75,18,N'user-vol-04',DATEADD(day,-7,GETDATE()),DATEADD(day,-7,GETUTCDATE())),
(76,18,N'user-vol-05',DATEADD(day,-8,GETDATE()),DATEADD(day,-8,GETUTCDATE())),
(77,19,N'user-vol-19',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(78,19,N'user-vol-20',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(79,19,N'user-vol-01',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(80,20,N'user-vol-20',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(81,20,N'user-vol-01',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(82,20,N'user-vol-02',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(83,21,N'user-vol-01',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(84,21,N'user-vol-02',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(85,21,N'user-vol-03',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(86,22,N'user-vol-02',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(87,22,N'user-vol-03',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(88,22,N'user-vol-04',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(89,23,N'user-vol-03',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(90,23,N'user-vol-04',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(91,23,N'user-vol-05',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(92,23,N'user-vol-06',DATEADD(day,-4,GETDATE()),DATEADD(day,-4,GETUTCDATE())),
(93,24,N'user-vol-04',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(94,24,N'user-vol-05',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(95,24,N'user-vol-06',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(96,25,N'user-vol-05',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(97,25,N'user-vol-06',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(98,25,N'user-vol-07',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(99,26,N'user-vol-06',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(100,26,N'user-vol-07',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(101,26,N'user-vol-08',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(102,27,N'user-vol-07',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(103,27,N'user-vol-08',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(104,27,N'user-vol-09',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(105,28,N'user-vol-08',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(106,28,N'user-vol-09',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(107,28,N'user-vol-10',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(108,29,N'user-vol-09',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(109,29,N'user-vol-10',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(110,29,N'user-vol-11',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE())),
(111,30,N'user-vol-10',DATEADD(day,-1,GETDATE()),DATEADD(day,-1,GETUTCDATE())),
(112,30,N'user-vol-11',DATEADD(day,-2,GETDATE()),DATEADD(day,-2,GETUTCDATE())),
(113,30,N'user-vol-12',DATEADD(day,-3,GETDATE()),DATEADD(day,-3,GETUTCDATE()));

SET IDENTITY_INSERT [EventRegistrations] OFF;

SET IDENTITY_INSERT [UserRatings] ON;

INSERT INTO [UserRatings] ([Id],[FromUserId],[ToUserId],[EventId],[Score],[ReviewText],[CreatedAt]) VALUES
(1,N'user-vol-01',N'user-org-01',1,4,N'Well organized and meaningful community event.',DATEADD(day,-1,GETUTCDATE())),
(2,N'user-vol-02',N'user-org-01',1,5,N'Clear instructions and friendly organizers.',DATEADD(day,-2,GETUTCDATE())),
(3,N'user-vol-03',N'user-org-01',1,4,N'A very useful project for the community.',DATEADD(day,-3,GETUTCDATE())),
(4,N'user-vol-02',N'user-org-02',2,5,N'Good coordination and volunteer support.',DATEADD(day,-1,GETUTCDATE())),
(5,N'user-vol-03',N'user-org-02',2,4,N'Great experience and helpful activity.',DATEADD(day,-2,GETUTCDATE())),
(6,N'user-vol-04',N'user-org-02',2,3,N'Well organized and meaningful community event.',DATEADD(day,-3,GETUTCDATE())),
(7,N'user-vol-03',N'user-org-03',3,4,N'Clear instructions and friendly organizers.',DATEADD(day,-1,GETUTCDATE())),
(8,N'user-vol-04',N'user-org-03',3,3,N'A very useful project for the community.',DATEADD(day,-2,GETUTCDATE())),
(9,N'user-vol-05',N'user-org-03',3,5,N'Good coordination and volunteer support.',DATEADD(day,-3,GETUTCDATE())),
(10,N'user-vol-04',N'user-org-04',4,3,N'Great experience and helpful activity.',DATEADD(day,-1,GETUTCDATE())),
(11,N'user-vol-05',N'user-org-04',4,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(12,N'user-vol-06',N'user-org-04',4,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(13,N'user-vol-05',N'user-org-05',5,5,N'A very useful project for the community.',DATEADD(day,-1,GETUTCDATE())),
(14,N'user-vol-06',N'user-org-05',5,4,N'Good coordination and volunteer support.',DATEADD(day,-2,GETUTCDATE())),
(15,N'user-vol-07',N'user-org-05',5,5,N'Great experience and helpful activity.',DATEADD(day,-3,GETUTCDATE())),
(16,N'user-vol-06',N'user-org-06',6,4,N'Well organized and meaningful community event.',DATEADD(day,-1,GETUTCDATE())),
(17,N'user-vol-07',N'user-org-06',6,5,N'Clear instructions and friendly organizers.',DATEADD(day,-2,GETUTCDATE())),
(18,N'user-vol-08',N'user-org-06',6,4,N'A very useful project for the community.',DATEADD(day,-3,GETUTCDATE())),
(19,N'user-vol-07',N'user-org-07',7,5,N'Good coordination and volunteer support.',DATEADD(day,-1,GETUTCDATE())),
(20,N'user-vol-08',N'user-org-07',7,4,N'Great experience and helpful activity.',DATEADD(day,-2,GETUTCDATE())),
(21,N'user-vol-09',N'user-org-07',7,3,N'Well organized and meaningful community event.',DATEADD(day,-3,GETUTCDATE())),
(22,N'user-vol-08',N'user-org-08',8,4,N'Clear instructions and friendly organizers.',DATEADD(day,-1,GETUTCDATE())),
(23,N'user-vol-09',N'user-org-08',8,3,N'A very useful project for the community.',DATEADD(day,-2,GETUTCDATE())),
(24,N'user-vol-10',N'user-org-08',8,5,N'Good coordination and volunteer support.',DATEADD(day,-3,GETUTCDATE())),
(25,N'user-vol-09',N'user-org-09',9,3,N'Great experience and helpful activity.',DATEADD(day,-1,GETUTCDATE())),
(26,N'user-vol-10',N'user-org-09',9,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(27,N'user-vol-11',N'user-org-09',9,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(28,N'user-vol-10',N'user-org-10',10,5,N'A very useful project for the community.',DATEADD(day,-1,GETUTCDATE())),
(29,N'user-vol-11',N'user-org-10',10,4,N'Good coordination and volunteer support.',DATEADD(day,-2,GETUTCDATE())),
(30,N'user-vol-12',N'user-org-10',10,5,N'Great experience and helpful activity.',DATEADD(day,-3,GETUTCDATE())),
(31,N'user-vol-11',N'user-org-11',11,4,N'Well organized and meaningful community event.',DATEADD(day,-1,GETUTCDATE())),
(32,N'user-vol-12',N'user-org-11',11,5,N'Clear instructions and friendly organizers.',DATEADD(day,-2,GETUTCDATE())),
(33,N'user-vol-13',N'user-org-11',11,4,N'A very useful project for the community.',DATEADD(day,-3,GETUTCDATE())),
(34,N'user-vol-12',N'user-org-12',12,5,N'Good coordination and volunteer support.',DATEADD(day,-1,GETUTCDATE())),
(35,N'user-vol-13',N'user-org-12',12,4,N'Great experience and helpful activity.',DATEADD(day,-2,GETUTCDATE())),
(36,N'user-vol-14',N'user-org-12',12,3,N'Well organized and meaningful community event.',DATEADD(day,-3,GETUTCDATE())),
(37,N'user-vol-13',N'user-org-13',13,4,N'Clear instructions and friendly organizers.',DATEADD(day,-1,GETUTCDATE())),
(38,N'user-vol-14',N'user-org-13',13,3,N'A very useful project for the community.',DATEADD(day,-2,GETUTCDATE())),
(39,N'user-vol-15',N'user-org-13',13,5,N'Good coordination and volunteer support.',DATEADD(day,-3,GETUTCDATE())),
(40,N'user-vol-14',N'user-org-14',14,3,N'Great experience and helpful activity.',DATEADD(day,-1,GETUTCDATE())),
(41,N'user-vol-15',N'user-org-14',14,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(42,N'user-vol-16',N'user-org-14',14,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(43,N'user-vol-15',N'user-org-15',15,5,N'A very useful project for the community.',DATEADD(day,-1,GETUTCDATE())),
(44,N'user-vol-16',N'user-org-15',15,4,N'Good coordination and volunteer support.',DATEADD(day,-2,GETUTCDATE())),
(45,N'user-vol-17',N'user-org-15',15,5,N'Great experience and helpful activity.',DATEADD(day,-3,GETUTCDATE())),
(46,N'user-vol-16',N'user-org-16',16,4,N'Well organized and meaningful community event.',DATEADD(day,-1,GETUTCDATE())),
(47,N'user-vol-17',N'user-org-16',16,5,N'Clear instructions and friendly organizers.',DATEADD(day,-2,GETUTCDATE())),
(48,N'user-vol-18',N'user-org-16',16,4,N'A very useful project for the community.',DATEADD(day,-3,GETUTCDATE())),
(49,N'user-vol-17',N'user-org-17',17,5,N'Good coordination and volunteer support.',DATEADD(day,-1,GETUTCDATE())),
(50,N'user-vol-18',N'user-org-17',17,4,N'Great experience and helpful activity.',DATEADD(day,-2,GETUTCDATE())),
(51,N'user-vol-19',N'user-org-17',17,3,N'Well organized and meaningful community event.',DATEADD(day,-3,GETUTCDATE())),
(52,N'user-vol-18',N'user-org-18',18,4,N'Clear instructions and friendly organizers.',DATEADD(day,-1,GETUTCDATE())),
(53,N'user-vol-19',N'user-org-18',18,3,N'A very useful project for the community.',DATEADD(day,-2,GETUTCDATE())),
(54,N'user-vol-20',N'user-org-18',18,5,N'Good coordination and volunteer support.',DATEADD(day,-3,GETUTCDATE())),
(55,N'user-vol-19',N'user-org-19',19,3,N'Great experience and helpful activity.',DATEADD(day,-1,GETUTCDATE())),
(56,N'user-vol-20',N'user-org-19',19,5,N'Well organized and meaningful community event.',DATEADD(day,-2,GETUTCDATE())),
(57,N'user-vol-01',N'user-org-19',19,4,N'Clear instructions and friendly organizers.',DATEADD(day,-3,GETUTCDATE())),
(58,N'user-vol-20',N'user-org-20',20,5,N'A very useful project for the community.',DATEADD(day,-1,GETUTCDATE())),
(59,N'user-vol-01',N'user-org-20',20,4,N'Good coordination and volunteer support.',DATEADD(day,-2,GETUTCDATE())),
(60,N'user-vol-02',N'user-org-20',20,5,N'Great experience and helpful activity.',DATEADD(day,-3,GETUTCDATE()));

SET IDENTITY_INSERT [UserRatings] OFF;

SET IDENTITY_INSERT [CommunityPosts] ON;

INSERT INTO [CommunityPosts] ([Id],[Title],[PostType],[Content],[UserProfileId],[CreatedAt]) VALUES
(1,N'Project Launch Success',N'Share',N'We successfully launched the OCVMS project today! Huge thanks to the development team.',2,DATEADD(day,-10,GETUTCDATE())),
(2,N'Beautiful Morning at the Park',N'Share',N'Spend my morning watering the new saplings. It’s peaceful to see them growing.',3,DATEADD(day,-9,GETUTCDATE())),
(3,N'New Milestone Reached',N'Share',N'Our community just hit 500 active volunteers! Thank you all for your dedication.',4,DATEADD(day,-8,GETUTCDATE())),
(4,N'Volunteer of the Month',N'Share',N'Congratulations to Kasun for being the most active volunteer this month!',5,DATEADD(day,-7,GETUTCDATE())),
(5,N'Quick Coding Tip',N'Share',N'For anyone working on the front end, using Flexbox makes responsive design much easier!',6,DATEADD(day,-6,GETUTCDATE())),
(6,N'Workshop Highlights',N'Share',N'Just finished the UI/UX workshop. Learned so much about user-centric design.',7,DATEADD(day,-5,GETUTCDATE())),
(7,N'Photo Gallery Update',N'Share',N'Check out the latest photos from last Saturday''s blood donation camp in the gallery.',8,DATEADD(day,-4,GETUTCDATE())),
(8,N'Inspiration for Today',N'Share',N'Remember, your smallest help might be someone else''s biggest hope. Keep up the good work!',9,DATEADD(day,-3,GETUTCDATE())),
(9,N'Meeting New People',N'Share',N'Met some amazing like-minded individuals during today’s food distribution drive.',10,DATEADD(day,-2,GETUTCDATE())),
(10,N'Looking Forward to Tomorrow',N'Share',N'Can''t wait for tomorrow''s orientation session. Ready to welcome the new batch!',11,DATEADD(day,-1,GETUTCDATE())),
(11,N'Bug Fixing Assistance',N'Help',N'I''m getting a NullReferenceException in my C# controller. Could anyone help me debug this?',7,DATEADD(day,-8,GETUTCDATE())),
(12,N'Graphic Designer Needed',N'Help',N'We need a simple flyer designed for the upcoming charity concert. Anyone available to help?',8,DATEADD(day,-7,GETUTCDATE())),
(13,N'Borrowing a Projector',N'Help',N'Does anyone have a portable projector I can borrow for our seminar on Friday?',9,DATEADD(day,-6,GETUTCDATE())),
(14,N'Urgent: Extra Hands Needed',N'Help',N'We are expecting a large crowd at the health clinic. Need 5 more volunteers for crowd control.',10,DATEADD(day,-5,GETUTCDATE())),
(15,N'Translation Help',N'Help',N'Need someone to help translate our event guidelines from English to Sinhala.',11,DATEADD(day,-4,GETUTCDATE())),
(16,N'Feedback on UI Design',N'Help',N'I just finished the dashboard mockup. Could someone review it and give me some feedback?',12,DATEADD(day,-3,GETUTCDATE())),
(17,N'Missing Equipment',N'Help',N'We lost a CAT6 ethernet cable at the last event. If anyone found it, please let me know.',13,DATEADD(day,-2,GETUTCDATE())),
(18,N'Laptop Charger Needed',N'Help',N'My Dell laptop charger just stopped working. Does anyone have a spare I can use until tomorrow?',14,DATEADD(day,-1,GETUTCDATE())),
(19,N'Research Participation',N'Help',N'I’m conducting a survey for my university project. Could you please take 2 minutes to fill it out?',15,DATEADD(day,-1,GETUTCDATE())),
(20,N'Documentation Help',N'Help',N'Looking for someone to help write the user manual for the volunteer management system.',16,DATEADD(day,-1,GETUTCDATE()));

SET IDENTITY_INSERT [CommunityPosts] OFF;

SET IDENTITY_INSERT [PostComments] ON;

INSERT INTO [PostComments] ([Id],[CommunityPostId],[UserProfileId],[Content],[CreatedAt]) VALUES
(1,1,3,N'I can help with this.',DATEADD(day,-9,GETUTCDATE())),
(2,2,4,N'Please share more details.',DATEADD(day,-8,GETUTCDATE())),
(3,2,5,N'Great initiative for the community.',DATEADD(day,-8,GETUTCDATE())),
(4,3,5,N'I am available during the weekend.',DATEADD(day,-7,GETUTCDATE())),
(5,4,6,N'This will be very useful for volunteers.',DATEADD(day,-6,GETUTCDATE())),
(6,4,7,N'Thank you for organizing this.',DATEADD(day,-6,GETUTCDATE())),
(7,5,7,N'I can help with this.',DATEADD(day,-5,GETUTCDATE())),
(8,6,8,N'Please share more details.',DATEADD(day,-4,GETUTCDATE())),
(9,6,9,N'Great initiative for the community.',DATEADD(day,-4,GETUTCDATE())),
(10,7,9,N'I am available during the weekend.',DATEADD(day,-3,GETUTCDATE())),
(11,8,10,N'This will be very useful for volunteers.',DATEADD(day,-2,GETUTCDATE())),
(12,8,11,N'Thank you for organizing this.',DATEADD(day,-2,GETUTCDATE())),
(13,9,11,N'I can help with this.',DATEADD(day,-1,GETUTCDATE())),
(14,10,12,N'Please share more details.',DATEADD(day,-1,GETUTCDATE())),
(15,10,13,N'Great initiative for the community.',DATEADD(day,-1,GETUTCDATE())),
(16,11,13,N'I am available during the weekend.',DATEADD(day,-1,GETUTCDATE())),
(17,12,14,N'This will be very useful for volunteers.',DATEADD(day,-1,GETUTCDATE())),
(18,12,15,N'Thank you for organizing this.',DATEADD(day,-1,GETUTCDATE())),
(19,13,15,N'I can help with this.',DATEADD(day,-1,GETUTCDATE())),
(20,14,16,N'Please share more details.',DATEADD(day,-1,GETUTCDATE())),
(21,14,17,N'Great initiative for the community.',DATEADD(day,-1,GETUTCDATE())),
(22,15,17,N'I am available during the weekend.',DATEADD(day,-1,GETUTCDATE()));

SET IDENTITY_INSERT [PostComments] OFF;

SET IDENTITY_INSERT [HelpRequests] ON;

INSERT INTO [HelpRequests] ([Id],[Title],[Description],[Status],[UserProfileId],[CreatedAt]) VALUES
(1,N'Bug Fixing Assistance',N'I''m getting a NullReferenceException in my C# controller. Could anyone help me debug this?',N'Pending',12,DATEADD(day,-1,GETUTCDATE())),
(2,N'Graphic Designer Needed',N'We need a simple flyer designed for the upcoming charity concert. Anyone available to help?',N'In Progress',13,DATEADD(day,-2,GETUTCDATE())),
(3,N'Borrowing a Projector',N'Does anyone have a portable projector I can borrow for our seminar on Friday?',N'Resolved',14,DATEADD(day,-3,GETUTCDATE())),
(4,N'Urgent: Extra Hands Needed',N'We are expecting a large crowd at the health clinic. Need 5 more volunteers for crowd control.',N'Pending',15,DATEADD(day,-4,GETUTCDATE())),
(5,N'Translation Help',N'Need someone to help translate our event guidelines from English to Sinhala.',N'In Progress',16,DATEADD(day,-5,GETUTCDATE())),
(6,N'Feedback on UI Design',N'I just finished the dashboard mockup. Could someone review it and give me some feedback?',N'Resolved',17,DATEADD(day,-6,GETUTCDATE())),
(7,N'Missing Equipment',N'We lost a CAT6 ethernet cable at the last event. If anyone found it, please let me know.',N'Pending',18,DATEADD(day,-7,GETUTCDATE())),
(8,N'Laptop Charger Needed',N'My Dell laptop charger just stopped working. Does anyone have a spare I can use until tomorrow?',N'In Progress',19,DATEADD(day,-8,GETUTCDATE())),
(9,N'Research Participation',N'I’m conducting a survey for my university project. Could you please take 2 minutes to fill it out?',N'Resolved',20,DATEADD(day,-9,GETUTCDATE())),
(10,N'Documentation Help',N'Looking for someone to help write the user manual for the volunteer management system.',N'Pending',21,DATEADD(day,-10,GETUTCDATE()));

SET IDENTITY_INSERT [HelpRequests] OFF;

SET IDENTITY_INSERT [Notifications] ON;

INSERT INTO [Notifications] ([Id],[UserProfileId],[Message],[IsRead],[CreatedAt]) VALUES
(1,2,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-2,GETUTCDATE())),
(2,3,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-3,GETUTCDATE())),
(3,4,N'Your organizer profile has been verified by the administrator.',1,DATEADD(day,-4,GETUTCDATE())),
(4,5,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-5,GETUTCDATE())),
(5,6,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-6,GETUTCDATE())),
(6,7,N'Your organizer profile has been verified by the administrator.',1,DATEADD(day,-7,GETUTCDATE())),
(7,8,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-8,GETUTCDATE())),
(8,9,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-9,GETUTCDATE())),
(9,10,N'Your organizer profile has been verified by the administrator.',1,DATEADD(day,-10,GETUTCDATE())),
(10,11,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-11,GETUTCDATE())),
(11,12,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-12,GETUTCDATE())),
(12,13,N'Your organizer profile has been verified by the administrator.',1,DATEADD(day,-1,GETUTCDATE())),
(13,14,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-2,GETUTCDATE())),
(14,15,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-3,GETUTCDATE())),
(15,16,N'Your organizer profile has been verified by the administrator.',1,DATEADD(day,-4,GETUTCDATE())),
(16,17,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-5,GETUTCDATE())),
(17,18,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-6,GETUTCDATE())),
(18,19,N'Your organizer profile has been verified by the administrator.',1,DATEADD(day,-7,GETUTCDATE())),
(19,20,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-8,GETUTCDATE())),
(20,21,N'Your organizer profile has been verified by the administrator.',0,DATEADD(day,-9,GETUTCDATE())),
(21,22,N'Your profile has been created and is ready for OCVMS demo testing.',1,DATEADD(day,-10,GETUTCDATE())),
(22,23,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-11,GETUTCDATE())),
(23,24,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-12,GETUTCDATE())),
(24,25,N'Your profile has been created and is ready for OCVMS demo testing.',1,DATEADD(day,-1,GETUTCDATE())),
(25,26,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-2,GETUTCDATE())),
(26,27,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-3,GETUTCDATE())),
(27,28,N'Your profile has been created and is ready for OCVMS demo testing.',1,DATEADD(day,-4,GETUTCDATE())),
(28,29,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-5,GETUTCDATE())),
(29,30,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-6,GETUTCDATE())),
(30,31,N'Your profile has been created and is ready for OCVMS demo testing.',1,DATEADD(day,-7,GETUTCDATE())),
(31,32,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-8,GETUTCDATE())),
(32,33,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-9,GETUTCDATE())),
(33,34,N'Your profile has been created and is ready for OCVMS demo testing.',1,DATEADD(day,-10,GETUTCDATE())),
(34,35,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-11,GETUTCDATE())),
(35,36,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-12,GETUTCDATE())),
(36,37,N'Your profile has been created and is ready for OCVMS demo testing.',1,DATEADD(day,-1,GETUTCDATE())),
(37,38,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-2,GETUTCDATE())),
(38,39,N'Your profile has been created and is ready for OCVMS demo testing.',0,DATEADD(day,-3,GETUTCDATE())),
(39,40,N'Your profile has been created and is ready for OCVMS demo testing.',1,DATEADD(day,-4,GETUTCDATE())),
(40,41,N'Your account is currently suspended for demonstration of admin ban feature.',0,DATEADD(day,-5,GETUTCDATE()));

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
