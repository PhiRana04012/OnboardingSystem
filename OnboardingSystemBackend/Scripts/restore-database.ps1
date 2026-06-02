param(
    [Parameter(Mandatory=$true)]
    [string]$ServerInstance,
    
    [Parameter(Mandatory=$true)]
    [string]$BackupFilePath,
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "onboarding_new",
    
    [Parameter(Mandatory=$false)]
    [switch]$VerifyOnly = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$Replace = $false
)

# Log function with file output
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO",
        [string]$LogFile = "restore_log.txt"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    Write-Host $logEntry
    
    try {
        Add-Content -Path $LogFile -Value $logEntry -ErrorAction SilentlyContinue
    }
    catch {
        # Log file write failed, continue anyway
    }
}

Write-Log "========================================" "START"
Write-Log "Database Restore Procedure Started" "START"
Write-Log "========================================" "START"
Write-Log "Server Instance: $ServerInstance"
Write-Log "Backup File: $BackupFilePath"
Write-Log "Target Database: $DatabaseName"
Write-Log "Verify Only: $VerifyOnly"
Write-Log "Replace Existing: $Replace"

try {
    # Step 1: Validate backup file exists
    Write-Log "Step 1: Validating backup file..." "INFO"
    
    if (-not (Test-Path $BackupFilePath)) {
        Write-Log "ERROR: Backup file not found: $BackupFilePath" "ERROR"
        exit 1
    }
    
    $backupFileSize = (Get-Item $BackupFilePath).Length / 1MB
    Write-Log "OK - Backup file found. Size: $([Math]::Round($backupFileSize, 2)) MB" "SUCCESS"
    
    # Step 2: Connect to SQL Server
    Write-Log "Step 2: Connecting to SQL Server..." "INFO"
    
    $connectionString = "Server=$ServerInstance;Database=master;Integrated Security=true;Connection Timeout=30"
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    
    try {
        $sqlConnection.Open()
        Write-Log "OK - Connected to SQL Server: $ServerInstance" "SUCCESS"
    }
    catch {
        Write-Log "ERROR: Failed to connect to SQL Server: $_" "ERROR"
        $sqlConnection.Close()
        exit 1
    }
    
    # Step 3: Verify backup file integrity using RESTORE VERIFYONLY
    Write-Log "Step 3: Verifying backup file integrity (RESTORE VERIFYONLY)..." "INFO"
    
    $backupFilePathEscaped = $BackupFilePath -replace '\\', '\\'
    $verifyQuery = "RESTORE VERIFYONLY FROM DISK = N'$backupFilePathEscaped'"
    
    try {
        $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($verifyQuery, $sqlConnection)
        $sqlCmd.CommandTimeout = 600  # 10 minute timeout for verify
        $sqlCmd.ExecuteNonQuery() | Out-Null
        Write-Log "OK - Backup file integrity verified successfully" "SUCCESS"
    }
    catch {
        Write-Log "ERROR: Backup file verification failed: $_" "ERROR"
        $sqlConnection.Close()
        exit 1
    }
    
    # If verify-only mode, exit here
    if ($VerifyOnly) {
        Write-Log "Verify-Only mode: stopping here. Backup is valid." "INFO"
        Write-Log "To proceed with restore, run without -VerifyOnly flag" "INFO"
        $sqlConnection.Close()
        Write-Log "========================================" "SUCCESS"
        exit 0
    }
    
    # Step 4: Check if database exists and handle replacement
    Write-Log "Step 4: Checking if database exists..." "INFO"
    
    $checkDbQuery = "SELECT 1 FROM sys.databases WHERE name = '$DatabaseName'"
    $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($checkDbQuery, $sqlConnection)
    $result = $sqlCmd.ExecuteScalar()
    
    if ($result) {
        Write-Log "Database already exists: $DatabaseName" "WARN"
        
        if ($Replace) {
            Write-Log "Replace flag set - will drop existing database" "WARN"
            
            try {
                $dropQuery = "ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$DatabaseName]"
                $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($dropQuery, $sqlConnection)
                $sqlCmd.CommandTimeout = 300
                $sqlCmd.ExecuteNonQuery() | Out-Null
                Write-Log "OK - Existing database dropped" "SUCCESS"
            }
            catch {
                Write-Log "ERROR: Failed to drop existing database: $_" "ERROR"
                $sqlConnection.Close()
                exit 1
            }
        }
        else {
            Write-Log "ERROR: Database exists and -Replace flag not set" "ERROR"
            Write-Log "Use -Replace flag to overwrite existing database" "INFO"
            $sqlConnection.Close()
            exit 1
        }
    }
    else {
        Write-Log "OK - Database does not exist, will be created during restore" "SUCCESS"
    }
    
    # Step 5: Execute RESTORE DATABASE
    Write-Log "Step 5: Executing RESTORE DATABASE..." "INFO"
    Write-Log "This may take several minutes..." "INFO"
    
    $restoreQuery = "RESTORE DATABASE [$DatabaseName] FROM DISK = N'$backupFilePathEscaped' WITH RECOVERY, REPLACE, STATS = 10"
    
    try {
        $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($restoreQuery, $sqlConnection)
        $sqlCmd.CommandTimeout = 3600  # 1 hour timeout
        
        $startTime = Get-Date
        $sqlCmd.ExecuteNonQuery() | Out-Null
        $endTime = Get-Date
        $restoreTime = ($endTime - $startTime).TotalSeconds
        
        Write-Log "OK - Restore completed successfully in $([Math]::Round($restoreTime, 2)) seconds" "SUCCESS"
    }
    catch {
        Write-Log "ERROR: Restore failed: $_" "ERROR"
        Write-Log "Stack Trace: $($_.ScriptStackTrace)" "ERROR"
        $sqlConnection.Close()
        exit 1
    }
    
    # Step 6: Post-restore verification
    Write-Log "Step 6: Post-restore verification..." "INFO"
    
    # Verify database is online
    $checkOnlineQuery = "SELECT state_desc FROM sys.databases WHERE name = '$DatabaseName'"
    $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($checkOnlineQuery, $sqlConnection)
    $dbState = $sqlCmd.ExecuteScalar()
    
    if ($dbState -eq "ONLINE") {
        Write-Log "OK - Database is ONLINE" "SUCCESS"
    }
    else {
        Write-Log "WARNING: Database state is $dbState (expected ONLINE)" "WARN"
    }
    
    # Verify basic schema
    $checkTablesQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
    $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($checkTablesQuery, $sqlConnection)
    $tableCount = $sqlCmd.ExecuteScalar()
    
    Write-Log "OK - Database contains $tableCount tables" "SUCCESS"
    
    # Verify critical tables exist
    $criticalTables = @("Users", "Modules", "Progress", "TestAttempts")
    $missingTables = @()
    
    foreach ($table in $criticalTables) {
        $checkTableQuery = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$table'"
        $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($checkTableQuery, $sqlConnection)
        $result = $sqlCmd.ExecuteScalar()
        
        if (-not $result) {
            $missingTables += $table
        }
    }
    
    if ($missingTables.Count -gt 0) {
        Write-Log "WARNING: Missing critical tables: $($missingTables -join ', ')" "WARN"
    }
    else {
        Write-Log "OK - All critical tables present" "SUCCESS"
    }
    
    $sqlConnection.Close()
    
    # Step 7: Success
    Write-Log "========================================" "SUCCESS"
    Write-Log "Database Restore Completed Successfully!" "SUCCESS"
    Write-Log "========================================" "SUCCESS"
    Write-Log "Database: $DatabaseName"
    Write-Log "Server: $ServerInstance"
    Write-Log "Restore Time: $([Math]::Round($restoreTime, 2)) seconds"
    Write-Log ""
    Write-Log "NEXT STEPS:"
    Write-Log "1. Verify application connectivity to database"
    Write-Log "2. Run application smoke tests"
    Write-Log "3. Check application logs for any errors"
    Write-Log "4. Validate business-critical data"
    Write-Log ""
    
    exit 0
}
catch {
    Write-Log "FATAL ERROR: $_" "ERROR"
    Write-Log "Stack Trace: $($_.ScriptStackTrace)" "ERROR"
    exit 1
}
