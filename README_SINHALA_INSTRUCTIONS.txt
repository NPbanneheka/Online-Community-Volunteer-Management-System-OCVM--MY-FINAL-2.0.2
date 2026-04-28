OCVMS Full Demo Data Package
===========================

මෙය OCVMS final demo/test සඳහා සකස් කළ clean demo data package එකක්.

මෙම package එකෙන් ලැබෙන දේවල්:
1. Meaningful users:
   - Admin
   - 6 Organizers
   - 12 Volunteers
   - 1 banned/suspended demo volunteer

2. Meaningful data:
   - 12 volunteer events
   - Event registrations
   - Organizer/event ratings
   - Community posts
   - Comments
   - Help requests
   - Notifications

3. Images:
   - Profile images: wwwroot/uploads/profiles
   - Event images: wwwroot/uploads/events

4. Backup:
   - Script එක run කළාම C:\Temp\OCVMS_FULL_DEMO_DATA.bak file එක auto create වෙනවා.

How to use
----------

Step 1:
මෙම ZIP එක extract කරන්න.

Step 2:
Extract වුණු demo_assets folder එක සහ Seed_OCVMS_Full_Demo_Data_And_Create_BAK.ps1 file එක
ඔයාගේ OCVMS project root folder එකට copy කරන්න.
Project root folder එක කියන්නේ OCVMS.csproj තියෙන folder එකයි.

Step 3:
App එක run වෙලා නම් stop කරන්න:
Ctrl + C

Step 4:
VS Code terminal එක project root folder එකේ open කරලා මේ command එක run කරන්න:

powershell -ExecutionPolicy Bypass -File .\Seed_OCVMS_Full_Demo_Data_And_Create_BAK.ps1

Step 5:
Script එක success වුණාම backup file එක මෙතන create වෙනවා:

C:\Temp\OCVMS_FULL_DEMO_DATA.bak

Step 6:
App එක run කරන්න:

dotnet run --project .\OCVMS.csproj

Important login accounts
------------------------

Admin:
admin@ocvms.local
Admin@2026#

Organizer:
nishan.green@ocvms.local
User@123

Organizer:
malki.lifecare@ocvms.local
User@123

Volunteer:
volunteer01@ocvms.local
User@123

Banned demo user:
volunteer12@ocvms.local
User@123

Notes
-----

- Script එක current OCVMSDb database data clear කරලා new demo data insert කරනවා.
- ඒ නිසා run කරන්න කලින් අවශ්‍ය නම් current DB backup එකක් තියාගන්න.
- C:\Temp එකේ ඔක්කොම delete කරන්න එපා. OCVMS related .bak files විතරක් manage කරන්න.
- The generated .bak can be restored later using your normal restore script.
