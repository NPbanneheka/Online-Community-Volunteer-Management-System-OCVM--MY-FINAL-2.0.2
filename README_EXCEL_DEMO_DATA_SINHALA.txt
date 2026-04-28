OCVMS Excel-Based Demo Data Package - Nishada

මෙම package එක ඔයා upload කළ Excel files තුනේ data අනුව සකස් කරලා තියෙනවා.

Included data:
- Admin account: 1
- Organizer accounts: 20
- Volunteer accounts: 20
- Volunteer events: 30
- Community share posts: 10
- Help/community request examples: 10
- Event registrations, ratings, comments, notifications
- Profile photos and event images

Run කරන විදිහ:
1. ZIP එක extract කරන්න.
2. පහත files/folders project root folder එකට copy කරන්න:
   - demo_assets
   - OCVMS_Demo_Data_From_Excel.sql
   - Seed_OCVMS_Excel_Demo_Data_And_Create_BAK.ps1
   - README_EXCEL_DEMO_DATA_SINHALA.txt

Project root folder කියන්නේ OCVMS.csproj සහ Program.cs තියෙන folder එක.

3. App එක run වෙලා නම් stop කරන්න:
   Ctrl + C

4. VS Code terminal එකේ project root folder එකෙන් run කරන්න:
   powershell -ExecutionPolicy Bypass -File .\Seed_OCVMS_Excel_Demo_Data_And_Create_BAK.ps1

5. Success වුණාම real SQL Server backup file එක මෙතන හැදෙයි:
   C:\Temp\OCVMS_EXCEL_DEMO_DATA.bak

6. App එක run කරන්න:
   dotnet run --project .\OCVMS.csproj

Login details:
Admin:
  Email: admin@ocvms.local
  Password: Admin@2026#

Organizers from Excel:
  Password: 123456
  Example: kasun.perera92@gmail.com / 123456

Volunteers from Excel:
  Password: 456789
  Example: amila.tennakoon84@gmail.com / 456789

Banned demo user:
  Email: malithi.kalu@gmail.com
  Password: 456789
  මෙය Admin Ban feature එක test කරන්න lockout කරලා තියෙන demo user කෙනෙක්.

Important:
මෙම script එක run කළාම current OCVMSDb data clear වෙලා මෙම Excel demo data insert වෙනවා.
Current data save කරගන්න ඕන නම් script run කරන්න කලින් backup එකක් තියාගන්න.
