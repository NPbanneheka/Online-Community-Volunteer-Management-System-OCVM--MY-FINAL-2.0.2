$ErrorActionPreference = "Stop"

$server = ".\SQLEXPRESS"
$database = "OCVMSDb"
$backupPath = "C:\Temp\OCVMS_EXCEL_DEMO_DATA.bak"
$sqlFile = ".\OCVMS_Demo_Data_From_Excel.sql"

if (!(Test-Path ".\OCVMS.csproj")) {
    Write-Host "ERROR: Please run this script from the OCVMS project root folder where OCVMS.csproj exists." -ForegroundColor Red
    exit 1
}

if (!(Test-Path $sqlFile)) {
    Write-Host "ERROR: OCVMS_Demo_Data_From_Excel.sql was not found in this folder." -ForegroundColor Red
    exit 1
}

New-Item -ItemType Directory -Force -Path "C:\Temp" | Out-Null
New-Item -ItemType Directory -Force -Path ".\wwwroot\uploads\profiles" | Out-Null
New-Item -ItemType Directory -Force -Path ".\wwwroot\uploads\events" | Out-Null

Write-Host "Copying demo profile and event images..." -ForegroundColor Cyan
Copy-Item ".\demo_assets\uploads\profiles\*" ".\wwwroot\uploads\profiles\" -Force
Copy-Item ".\demo_assets\uploads\events\*" ".\wwwroot\uploads\events\" -Force

$connectionString = "Server=$server;Database=$database;Trusted_Connection=True;TrustServerCertificate=True"
$sql = Get-Content $sqlFile -Raw -Encoding UTF8

Write-Host "Resetting OCVMSDb and inserting Excel-based demo data..." -ForegroundColor Cyan
$conn = New-Object System.Data.SqlClient.SqlConnection($connectionString)
$cmd = $conn.CreateCommand()
$cmd.CommandText = $sql
$cmd.CommandTimeout = 900

$conn.Open()
$reader = $cmd.ExecuteReader()

$table = New-Object System.Data.DataTable
$table.Load($reader)
$conn.Close()

$table | Format-Table -AutoSize

Write-Host "Creating SQL Server backup file..." -ForegroundColor Cyan

$masterConnectionString = "Server=$server;Database=master;Trusted_Connection=True;TrustServerCertificate=True"
$backupSql = @"
BACKUP DATABASE [$database]
TO DISK = N'$backupPath'
WITH INIT, FORMAT, STATS = 5;
"@

$conn = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
$cmd = $conn.CreateCommand()
$cmd.CommandText = $backupSql
$cmd.CommandTimeout = 600

$conn.Open()
$cmd.ExecuteNonQuery()
$conn.Close()

Write-Host "Done!" -ForegroundColor Green
Write-Host "Backup created at: $backupPath" -ForegroundColor Yellow
Write-Host "Admin login: admin@ocvms.local / Admin@2026#" -ForegroundColor Yellow
Write-Host "Organizer logins use password: 123456" -ForegroundColor Yellow
Write-Host "Volunteer logins use password: 456789" -ForegroundColor Yellow
