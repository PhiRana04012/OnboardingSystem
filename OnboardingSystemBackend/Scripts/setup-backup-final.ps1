param()

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "Error: Script must be run as administrator!" -ForegroundColor Red
    exit 1
}

Write-Host "OK - Script is running as administrator" -ForegroundColor Green
Write-Host ""

# Configuration
$taskName = "OnboardingSystem_DatabaseBackup"
$scriptPath = "C:\Users\Firan\ONBOARDING\OnboardingSystemBackend\Scripts\backup-database.ps1"
$backupPath = "C:\Backups\OnboardingSystem"
$runTime = "02:00"

Write-Host "Configuration:" -ForegroundColor Cyan
Write-Host "  Task name: $taskName"
Write-Host "  Run time: $runTime (daily)"
Write-Host "  Script path: $scriptPath"
Write-Host "  Backup folder: $backupPath"
Write-Host ""

# Step 1: Check backup script exists
Write-Host "Step 1: Checking backup script..." -ForegroundColor Yellow

if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: File not found: $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "OK - File found" -ForegroundColor Green
Write-Host ""

# Step 2: Create backup directory
Write-Host "Step 2: Creating backup directory..." -ForegroundColor Yellow

if (-not (Test-Path $backupPath)) {
    New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    Write-Host "OK - Directory created: $backupPath" -ForegroundColor Green
}
else {
    Write-Host "OK - Directory already exists" -ForegroundColor Green
}

Write-Host ""

# Step 3: Check SQL Server SMO
Write-Host "Step 3: Checking SQL Server Management Objects (SMO)..." -ForegroundColor Yellow

try {
    [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo") | Out-Null
    Write-Host "OK - SQL Server SMO is installed" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: SQL Server SMO not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install:"
    Write-Host "  1. SQL Server Management Studio (SSMS)"
    Write-Host "  2. Or SQL Server Express with Tools"
    Write-Host ""
    Write-Host "Download: https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms"
    exit 1
}

Write-Host ""

# Step 4: Check if task already exists
Write-Host "Step 4: Checking for existing task..." -ForegroundColor Yellow

$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

if ($existingTask) {
    Write-Host "Found existing task - removing old task"
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false | Out-Null
    Write-Host "OK - Old task removed" -ForegroundColor Green
    Start-Sleep -Seconds 1
}
else {
    Write-Host "OK - No existing task found" -ForegroundColor Green
}

Write-Host ""

# Step 5: Create new task
Write-Host "Step 5: Creating new task in Task Scheduler..." -ForegroundColor Yellow

try {
    # Action
    $action = New-ScheduledTaskAction `
        -Execute "powershell.exe" `
        -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`" -ServerInstance 'localhost\SQLEXPRESS' -DatabaseName 'onboarding_new' -BackupPath '$backupPath' -RetentionDays 30"

    # Trigger
    $trigger = New-ScheduledTaskTrigger -Daily -At $runTime

    # Settings
    $settings = New-ScheduledTaskSettingsSet `
        -AllowStartIfOnBatteries $false `
        -Compatibility Win8 `
        -ExecutionTimeLimit (New-TimeSpan -Hours 2) `
        -RestartCount 2 `
        -RestartInterval (New-TimeSpan -Minutes 1) `
        -StartWhenAvailable $true

    # Principal
    $principal = New-ScheduledTaskPrincipal `
        -UserId "SYSTEM" `
        -RunLevel Highest

    # Register task
    Register-ScheduledTask `
        -TaskName $taskName `
        -Action $action `
        -Trigger $trigger `
        -Settings $settings `
        -Principal $principal `
        -Description "Daily database backup for OnboardingSystem" `
        -Force | Out-Null

    Write-Host "OK - Task created successfully" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to create task: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 6: Verify task
Write-Host "Step 6: Verifying task..." -ForegroundColor Yellow

$task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue

if ($task) {
    Write-Host "OK - Task is registered" -ForegroundColor Green
    Write-Host ""
    Write-Host "Task details:" -ForegroundColor Cyan
    Write-Host "  Name: $($task.TaskName)"
    Write-Host "  Status: $($task.State)"
    Write-Host "  Schedule: Daily at $runTime"
    Write-Host ""
}
else {
    Write-Host "ERROR: Task not found after creation" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 7: Test run (optional)
Write-Host "Step 7: Test backup (optional)" -ForegroundColor Yellow
Write-Host ""
Write-Host "Run first backup now?" -ForegroundColor Cyan
Write-Host "(Answer 'y' to test or 'n' to skip): " -NoNewline

$response = Read-Host

if ($response -eq 'y' -or $response -eq 'Y') {
    Write-Host ""
    Write-Host "Running first backup (this may take 2-3 minutes)..." -ForegroundColor Cyan
    Write-Host ""
    
    try {
        & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $scriptPath `
            -ServerInstance "localhost\SQLEXPRESS" `
            -DatabaseName "onboarding_new" `
            -BackupPath $backupPath `
            -RetentionDays 30
        
        Write-Host ""
        Write-Host "OK - First backup completed successfully!" -ForegroundColor Green
    }
    catch {
        Write-Host ""
        Write-Host "ERROR during backup: $_" -ForegroundColor Red
    }
}
else {
    Write-Host "Skipping test backup" -ForegroundColor Yellow
}

Write-Host ""

# Final summary
Write-Host "======================================" -ForegroundColor Green
Write-Host "SUCCESS - Setup completed!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host ""

Write-Host "What happens next:" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Backup runs automatically every day at 02:00"
Write-Host "2. Files are stored in: C:\Backups\OnboardingSystem\"
Write-Host "3. Old backups (>30 days) are deleted automatically"
Write-Host "4. All operations are logged in backup_log.txt"
Write-Host ""

Write-Host "Useful commands:" -ForegroundColor Cyan
Write-Host "  Check task:  Get-ScheduledTask -TaskName `"$taskName`""
Write-Host "  Run now:     Start-ScheduledTask -TaskName `"$taskName`""
Write-Host "  View logs:   Get-Content C:\Backups\OnboardingSystem\backup_log.txt -Tail 50"
Write-Host "  Delete task: Unregister-ScheduledTask -TaskName `"$taskName`""
Write-Host ""

Write-Host "======================================" -ForegroundColor Green
