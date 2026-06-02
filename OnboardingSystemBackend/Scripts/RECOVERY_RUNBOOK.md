# Database Recovery Runbook
## OnboardingSystem - Critical Failure Recovery Procedure

**Objective:** Recover the OnboardingSystem database to operational state within 4 hours (RTO ≤ 4h).

**Target Recovery Time (RTO):** 4 hours  
**Recovery Point Objective (RPO):** 24 hours (previous day's backup)

---

## Quick Reference

| Phase | Duration | Owner |
|-------|----------|-------|
| Assessment & Preparation | 15 min | Ops Lead |
| Environment Setup | 45 min | Infrastructure |
| Database Restore | 30-60 min | DBA |
| Application Verification | 30 min | Dev/QA |
| Business Validation | 30 min | Business Owner |
| **Total** | **< 4 hours** | |

---

## Prerequisites

Before executing this runbook, ensure the following are in place:

### Infrastructure & Access
- [ ] Administrative access to source backup server (where `.bak` files are stored)
- [ ] Administrative access to target SQL Server instance (`localhost\SQLEXPRESS` or configured instance)
- [ ] Network connectivity between systems
- [ ] PowerShell execution policy set to allow script execution:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```
- [ ] SQL Server Management Objects (SMO) installed (SQL Server Management Studio or Express Tools)

### Backup Artifacts
- [ ] Backup files are available in configured directory (default: `C:\Backups\OnboardingSystem\`)
- [ ] Backup files are not corrupted (verified via `verify-backups.ps1`)
- [ ] At least one valid `.bak` file exists (recommended: choose the most recent)

### Application Setup
- [ ] Application code deployed and ready (or can be redeployed quickly)
- [ ] Application configuration files ready with correct database connection strings
- [ ] Smoke test suite ready (API endpoints, authentication, basic queries)

---

## Phase 1: Assessment & Preparation (15 minutes)

### 1.1 Assess the Failure
- **Determine failure type:** Database server down? Corrupted database? Data loss?
- **Identify scope:** Is only the database affected, or entire system?
- **Document incident time** and last known good state

**Commands:**
```powershell
# Check if SQL Server is running
Get-Service -Name "MSSQL*" | Select-Object Name, Status

# Try to connect to database
sqlcmd -S localhost\SQLEXPRESS -Q "SELECT @@VERSION"

# If connection fails, service is stopped, or database is inaccessible, proceed to restore
```

### 1.2 Notify Stakeholders
- [ ] Inform team lead and business owner of outage
- [ ] Start incident tracking (timestamp, severity, scope)
- [ ] Brief the recovery team on timeline and expectations

### 1.3 Gather Backup Information
```powershell
# List available backup files (most recent first)
Get-ChildItem -Path "C:\Backups\OnboardingSystem\" -Filter "*.bak" | Sort-Object LastWriteTime -Descending | Select-Object Name, Length, LastWriteTime -First 5

# Example output:
# Name                                          Length         LastWriteTime
# ----                                          ------         -----
# onboarding_new_20260527_020000_backup.bak     1234567890     5/27/2026 2:00:00 AM
# onboarding_new_20260526_020000_backup.bak     1234567890     5/26/2026 2:00:00 AM
```

**Decision:** Choose the most recent valid backup file (typically the previous day's 02:00 backup).

---

## Phase 2: Environment Setup (45 minutes)

### 2.1 Prepare Target SQL Server
```powershell
# Option A: If using existing SQL Server instance
# 1. Verify SQL Server is running and accessible
$sqlConnection = New-Object System.Data.SqlClient.SqlConnection("Server=localhost\SQLEXPRESS;Database=master;Integrated Security=true")
try {
    $sqlConnection.Open()
    Write-Host "OK - SQL Server is accessible"
} catch {
    Write-Host "ERROR - Cannot connect: $_"
    # Start SQL Server service if stopped
    Start-Service -Name "MSSQL`$SQLEXPRESS" -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 30  # Wait for service to start
}
$sqlConnection.Close()

# Option B: If deploying new SQL Server instance
# Use Docker or VM template (estimated 20-30 minutes):
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourPass123!" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

### 2.2 Prepare Storage & Paths
```powershell
# Ensure backup file is accessible on target system
$backupFile = "C:\Backups\OnboardingSystem\onboarding_new_20260527_020000_backup.bak"
if (-not (Test-Path $backupFile)) {
    Write-Host "ERROR: Backup file not found!"
    # Option: Copy from remote backup location
    # Copy-Item -Path "\\backup-server\backups\onboarding_new_20260527_020000_backup.bak" -Destination $backupFile
    exit 1
}
Write-Host "OK - Backup file found: $backupFile"
```

---

## Phase 3: Database Restore (30-60 minutes)

### 3.1 Verify Backup Integrity

Run the verification script to ensure backup is not corrupted:

```powershell
# Run with administrator privileges
.\verify-backups.ps1 -ServerInstance "localhost\SQLEXPRESS" -BackupsToCheck 3

# Expected output: All backups should show status "VALID"
```

### 3.2 Execute Restore (Dry Run - Optional)

First, verify without making changes:

```powershell
.\restore-database.ps1 `
    -ServerInstance "localhost\SQLEXPRESS" `
    -BackupFilePath "C:\Backups\OnboardingSystem\onboarding_new_20260527_020000_backup.bak" `
    -DatabaseName "onboarding_new" `
    -VerifyOnly

# Expected output:
# [2026-05-27 ...] [SUCCESS] OK - Backup file found...
# [2026-05-27 ...] [SUCCESS] OK - Connected to SQL Server...
# [2026-05-27 ...] [SUCCESS] OK - Backup file integrity verified successfully
# [2026-05-27 ...] [INFO] Verify-Only mode: stopping here...
```

### 3.3 Execute Full Restore

Run the restore with `-Replace` flag to overwrite existing database (if needed):

```powershell
.\restore-database.ps1 `
    -ServerInstance "localhost\SQLEXPRESS" `
    -BackupFilePath "C:\Backups\OnboardingSystem\onboarding_new_20260527_020000_backup.bak" `
    -DatabaseName "onboarding_new" `
    -Replace

# Expected output:
# [2026-05-27 ...] [SUCCESS] OK - Backup file found. Size: 1200.50 MB
# [2026-05-27 ...] [SUCCESS] OK - Connected to SQL Server: localhost\SQLEXPRESS
# [2026-05-27 ...] [INFO] Executing RESTORE DATABASE...
# [2026-05-27 ...] [INFO] This may take several minutes...
# [2026-05-27 ...] [SUCCESS] OK - Restore completed successfully in 1247.35 seconds
# [2026-05-27 ...] [SUCCESS] OK - Database is ONLINE
# [2026-05-27 ...] [SUCCESS] OK - Database contains 27 tables
# [2026-05-27 ...] [SUCCESS] OK - All critical tables present
# [2026-05-27 ...] [SUCCESS] Database Restore Completed Successfully!
```

**Timeline Estimate:**
- Small database (< 100 MB): 2-5 minutes
- Medium database (100 MB - 1 GB): 10-20 minutes
- Large database (> 1 GB): 20-60 minutes

### 3.4 Verify Database State

```powershell
# Connect to restored database and run quick checks
sqlcmd -S localhost\SQLEXPRESS -d onboarding_new -Q "
SELECT 'Database State' as Check, state_desc FROM sys.databases WHERE name = 'onboarding_new'
GO
SELECT 'Table Count' as Check, COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'
GO
SELECT 'User Count' as Check, COUNT(*) FROM [Users]
GO
SELECT 'Module Count' as Check, COUNT(*) FROM [Modules]
GO
SELECT 'Progress Count' as Check, COUNT(*) FROM [Progress]
GO
"

# Expected output: Database should be ONLINE, tables should have counts > 0
```

---

## Phase 4: Application Verification (30 minutes)

### 4.1 Update Connection Strings

Update application configuration to point to recovered database:

**File:** `OnboardingSystemBackend/appsettings.json` (or environment-specific)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=onboarding_new;Integrated Security=true;"
  }
}
```

Or via environment variable:
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost\SQLEXPRESS;Database=onboarding_new;Integrated Security=true;"
```

### 4.2 Restart Application Services

```powershell
# If running as Docker container:
docker restart onboarding-backend

# If running as Windows service:
Stop-Service -Name "OnboardingSystem" -Force
Start-Service -Name "OnboardingSystem"

# If running directly (development):
# Restart the dotnet application manually or via CI/CD pipeline
```

### 4.3 Run Smoke Tests

Execute basic health checks:

```powershell
# Test 1: Health endpoint
$response = Invoke-WebRequest -Uri "http://localhost:5233/health" -Method Get
if ($response.StatusCode -eq 200) {
    Write-Host "✓ Health endpoint OK"
} else {
    Write-Host "✗ Health endpoint failed"
}

# Test 2: Authentication
$response = Invoke-RestMethod `
    -Uri "http://localhost:5233/api/users/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body @{ email = "test@example.com"; password = "password" } `
    -ErrorAction SilentlyContinue

if ($response.success) {
    Write-Host "✓ Authentication working"
} else {
    Write-Host "✗ Authentication failed"
}

# Test 3: Database connectivity
$response = Invoke-WebRequest -Uri "http://localhost:5233/api/modules" -Method Get -ErrorAction SilentlyContinue
if ($response.StatusCode -eq 200) {
    Write-Host "✓ API endpoint responding"
} else {
    Write-Host "✗ API endpoint not responding"
}

# Test 4: Check application logs
Get-Content "OnboardingSystemBackend\logs\*.log" -Tail 50 | Select-String -Pattern "ERROR|WARNING" -NotMatch | Select-String "Started|Connection|Database"
```

---

## Phase 5: Business Validation (30 minutes)

### 5.1 Checklist for Business Owner / Product Manager

- [ ] Can log in with valid credentials?
- [ ] Can view onboarding dashboard and modules?
- [ ] Can see mentee assignments (if applicable)?
- [ ] Can access reports and analytics?
- [ ] Can upload files and attachments (if applicable)?
- [ ] Can see historical data from before the outage?
- [ ] Are there any obvious data inconsistencies?
- [ ] Can create new records and changes persist?

### 5.2 Run Business-Critical Scenarios

```
Scenario 1: New Employee Onboarding Flow
- Create new user account
- Assign modules
- Generate initial report
- Verify progress tracking

Scenario 2: Mentor Assignment
- Assign mentor to new employee
- Check notification delivery
- Verify mentor can see mentees

Scenario 3: Module Completion & Testing
- Complete a module
- Submit test
- Check score recording
- Verify analytics update

Scenario 4: Report Generation
- Generate progress report
- Generate analytics report
- Export to Excel/PDF
- Verify data accuracy
```

---

## Success Criteria

Recovery is successful when:

1. ✅ Database is ONLINE and accessible
2. ✅ All critical tables present with data
3. ✅ Application starts without errors
4. ✅ All smoke tests pass
5. ✅ Users can log in and perform basic operations
6. ✅ Historical data is visible and correct
7. ✅ No data loss beyond expected RPO (last 24 hours)

---

## Rollback Procedure (if recovery fails)

If restore fails partway through:

```powershell
# Option 1: Try restore again with different backup file
.\restore-database.ps1 `
    -ServerInstance "localhost\SQLEXPRESS" `
    -BackupFilePath "C:\Backups\OnboardingSystem\onboarding_new_20260526_020000_backup.bak" `
    -DatabaseName "onboarding_new" `
    -Replace

# Option 2: If database is corrupted, drop and retry
sqlcmd -S localhost\SQLEXPRESS -Q "
ALTER DATABASE [onboarding_new] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
DROP DATABASE [onboarding_new]
GO
"

# Then re-run restore
.\restore-database.ps1 -ServerInstance "localhost\SQLEXPRESS" -BackupFilePath "..." -DatabaseName "onboarding_new"

# Option 3: If all backups fail, restore from off-site backup or vendor support
```

---

## Post-Recovery Actions (Within 24 hours)

- [ ] Document incident: what failed, why, how we recovered
- [ ] Update runbook with lessons learned
- [ ] Verify new backups are running correctly after recovery
- [ ] Run full backup cycle verification
- [ ] Check database integrity: `DBCC CHECKDB('onboarding_new')`
- [ ] Review application logs for any errors during recovery
- [ ] Schedule post-incident review meeting with team
- [ ] Test recovery procedure one more time to confirm it works

---

## Troubleshooting

### Problem: "Cannot connect to SQL Server"
**Solution:**
```powershell
# Check if service is running
Get-Service -Name "MSSQL*"
# Start service if stopped
Start-Service -Name "MSSQL$SQLEXPRESS"
# Verify connectivity
sqlcmd -S localhost\SQLEXPRESS -Q "SELECT @@VERSION"
```

### Problem: "Backup file corrupted or unreadable"
**Solution:**
```powershell
# Verify backup
.\verify-backups.ps1 -ServerInstance "localhost\SQLEXPRESS"
# Try older backup
Get-ChildItem -Path "C:\Backups\OnboardingSystem\" -Filter "*.bak" | Sort-Object LastWriteTime -Descending
# Copy from off-site if available
```

### Problem: "Database already exists"
**Solution:**
```powershell
# Use -Replace flag
.\restore-database.ps1 ... -Replace

# Or manually drop:
sqlcmd -S localhost\SQLEXPRESS -Q "
ALTER DATABASE [onboarding_new] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
DROP DATABASE [onboarding_new]
GO
"
```

### Problem: "Restore timed out"
**Solution:**
- Increase timeout in script (change `$sqlCmd.CommandTimeout = 3600` to larger value)
- Check disk I/O on SQL Server
- Reduce other workload on server during restore
- Verify backup file is on local fast storage

---

## Key Contacts & Escalation

| Role | Name | Phone | Email |
|------|------|-------|-------|
| DBA / Recovery Lead | [Name] | [Phone] | [Email] |
| Infrastructure Lead | [Name] | [Phone] | [Email] |
| Application Owner | [Name] | [Phone] | [Email] |
| Business Owner | [Name] | [Phone] | [Email] |

---

## References

- **Backup Script:** `OnboardingSystemBackend/Scripts/backup-database.ps1`
- **Restore Script:** `OnboardingSystemBackend/Scripts/restore-database.ps1`
- **Verification Script:** `OnboardingSystemBackend/Scripts/verify-backups.ps1`
- **Setup Script:** `OnboardingSystemBackend/Scripts/setup-backup-final.ps1`
- **SQL Server Restore Docs:** https://learn.microsoft.com/en-us/sql/t-sql/statements/restore-statements-transact-sql

---

**Last Updated:** May 27, 2026  
**Runbook Version:** 1.0
