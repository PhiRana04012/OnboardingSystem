param(
    [Parameter(Mandatory=$true)]
    [string]$ServerInstance,
    
    [Parameter(Mandatory=$false)]
    [int]$BackupsToCheck = 5
)

# Log function
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] [$Level] $Message"
}

Write-Log "========================================" "START"
Write-Log "Backup Verification Report" "START"
Write-Log "========================================" "START"

try {
    # Connect to SQL Server
    $connectionString = "Server=$ServerInstance;Database=master;Integrated Security=true;Connection Timeout=30"
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $sqlConnection.Open()
    Write-Log "Connected to SQL Server: $ServerInstance" "SUCCESS"
    
    # Get list of backup files
    $backupPath = "C:\Backups\OnboardingSystem"
    
    if (-not (Test-Path $backupPath)) {
        Write-Log "ERROR: Backup directory not found: $backupPath" "ERROR"
        $sqlConnection.Close()
        exit 1
    }
    
    Write-Log "Backup directory: $backupPath" "INFO"
    Write-Log ""
    
    # Get latest backup files
    $backupFiles = Get-ChildItem -Path $backupPath -Filter "*.bak" | Sort-Object LastWriteTime -Descending | Select-Object -First $BackupsToCheck
    
    if ($backupFiles.Count -eq 0) {
        Write-Log "ERROR: No backup files found" "ERROR"
        $sqlConnection.Close()
        exit 1
    }
    
    Write-Log "Found $($backupFiles.Count) recent backup files:" "INFO"
    Write-Log ""
    
    $resultsTable = @()
    
    foreach ($backupFile in $backupFiles) {
        $fileName = $backupFile.Name
        $filePath = $backupFile.FullName
        $fileSize = $backupFile.Length / 1MB
        $lastModified = $backupFile.LastWriteTime
        
        Write-Log "Checking: $fileName" "INFO"
        Write-Log "  Size: $([Math]::Round($fileSize, 2)) MB" "INFO"
        Write-Log "  Modified: $lastModified" "INFO"
        
        # Verify backup using SQL
        $backupFilePathEscaped = $filePath -replace '\\', '\\'
        $verifyQuery = "RESTORE VERIFYONLY FROM DISK = N'$backupFilePathEscaped'"
        
        try {
            $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($verifyQuery, $sqlConnection)
            $sqlCmd.CommandTimeout = 300
            $sqlCmd.ExecuteNonQuery() | Out-Null
            
            Write-Log "  Status: OK - Backup is valid" "SUCCESS"
            
            $resultsTable += [PSCustomObject]@{
                FileName = $fileName
                Size_MB = [Math]::Round($fileSize, 2)
                LastModified = $lastModified
                Status = "VALID"
                Error = ""
            }
        }
        catch {
            $errorMsg = $_.Exception.Message
            Write-Log "  Status: FAILED - $errorMsg" "ERROR"
            
            $resultsTable += [PSCustomObject]@{
                FileName = $fileName
                Size_MB = [Math]::Round($fileSize, 2)
                LastModified = $lastModified
                Status = "INVALID"
                Error = $errorMsg
            }
        }
        
        Write-Log ""
    }
    
    $sqlConnection.Close()
    
    # Summary
    Write-Log "========================================" "INFO"
    Write-Log "Summary" "INFO"
    Write-Log "========================================" "INFO"
    
    $validCount = ($resultsTable | Where-Object { $_.Status -eq "VALID" }).Count
    $invalidCount = ($resultsTable | Where-Object { $_.Status -eq "INVALID" }).Count
    
    Write-Log "Total checked: $($resultsTable.Count)" "INFO"
    Write-Log "Valid backups: $validCount" "SUCCESS"
    Write-Log "Invalid backups: $invalidCount" "ERROR"
    Write-Log ""
    
    # Display table
    Write-Log "Detailed Results:" "INFO"
    $resultsTable | Format-Table -Property FileName, Size_MB, LastModified, Status -AutoSize
    
    # Retention check
    Write-Log ""
    Write-Log "Retention Policy Check:" "INFO"
    
    $cutoffDate = (Get-Date).AddDays(-30)
    $retentionViolations = Get-ChildItem -Path $backupPath -Filter "*.bak" | Where-Object { $_.LastWriteTime -lt $cutoffDate }
    
    if ($retentionViolations.Count -gt 0) {
        Write-Log "WARNING: Found $($retentionViolations.Count) backup files older than 30 days (should be deleted)" "WARN"
        Write-Log "These files should be cleaned by the daily backup task:" "WARN"
        foreach ($file in $retentionViolations) {
            Write-Log "  - $($file.Name) (Modified: $($file.LastWriteTime))" "WARN"
        }
    }
    else {
        Write-Log "OK - Retention policy is being enforced (no files older than 30 days)" "SUCCESS"
    }
    
    Write-Log ""
    Write-Log "========================================" "INFO"
    
    if ($invalidCount -eq 0) {
        Write-Log "All backups verified successfully!" "SUCCESS"
        exit 0
    }
    else {
        Write-Log "WARNING: Some backups failed verification" "ERROR"
        exit 1
    }
}
catch {
    Write-Log "FATAL ERROR: $_" "ERROR"
    exit 1
}
