param(
    [Parameter(Mandatory=$true)]
    [string]$ServerInstance,
    
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,
    
    [Parameter(Mandatory=$true)]
    [string]$BackupPath,
    
    [Parameter(Mandatory=$true)]
    [int]$RetentionDays
)

# Log function
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] [$Level] $Message"
}

Write-Log "Starting database backup process"
Write-Log "Server: $ServerInstance"
Write-Log "Database: $DatabaseName"
Write-Log "Backup Path: $BackupPath"
Write-Log "Retention: $RetentionDays days"

# Check backup directory exists
if (-not (Test-Path $BackupPath)) {
    Write-Log "Creating backup directory: $BackupPath" "WARN"
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
}

try {
    # Create connection string
    $connectionString = "Server=$ServerInstance;Database=master;Integrated Security=true;Connection Timeout=30"
    $sqlConnection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    
    Write-Log "Connecting to SQL Server instance: $ServerInstance"
    $sqlConnection.Open()
    Write-Log "Connected to SQL Server successfully"
    
    # Check if database exists
    $checkDbQuery = "SELECT 1 FROM sys.databases WHERE name = '$DatabaseName'"
    $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($checkDbQuery, $sqlConnection)
    $result = $sqlCmd.ExecuteScalar()
    
    if (-not $result) {
        Write-Log "Database not found: $DatabaseName" "ERROR"
        $sqlConnection.Close()
        exit 1
    }
    
    Write-Log "Database found: $DatabaseName"
    
    # Create backup filename with timestamp
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFileName = "{0}_{1}_backup.bak" -f $DatabaseName, $timestamp
    $backupFilePath = Join-Path $BackupPath $backupFileName
    
    # Escape backslashes for SQL
    $backupFilePathEscaped = $backupFilePath -replace '\\', '\\'
    
    Write-Log "Starting backup to: $backupFilePath"
    
    # Execute BACKUP DATABASE command
    $backupQuery = "BACKUP DATABASE [$DatabaseName] TO DISK = N'$backupFilePathEscaped' WITH NOFORMAT, NOINIT, NAME = N'Full Backup of $DatabaseName', SKIP, STATS = 10, CHECKSUM"
    
    $sqlCmd = New-Object System.Data.SqlClient.SqlCommand($backupQuery, $sqlConnection)
    $sqlCmd.CommandTimeout = 3600  # 1 hour timeout
    $sqlCmd.ExecuteNonQuery()
    
    $sqlConnection.Close()
    
    Write-Log "Backup completed successfully"
    Write-Log "Backup file: $backupFilePath"
    Write-Log "File size: $((Get-Item $backupFilePath).Length / 1MB) MB"
    
    # Cleanup old backups
    Write-Log "Cleaning up old backups (retention: $RetentionDays days)"
    
    $cutoffDate = (Get-Date).AddDays(-$RetentionDays)
    $oldBackups = Get-ChildItem -Path $BackupPath -Filter "*$DatabaseName*backup*.bak" | Where-Object { $_.LastWriteTime -lt $cutoffDate }
    
    if ($oldBackups) {
        foreach ($backup in $oldBackups) {
            Write-Log "Removing old backup: $($backup.Name)" "WARN"
            Remove-Item $backup.FullName -Force
        }
        Write-Log "Removed $($oldBackups.Count) old backup file(s)"
    }
    else {
        Write-Log "No old backups to remove"
    }
    
    Write-Log "Backup process completed successfully" "SUCCESS"
    exit 0
}
catch {
    Write-Log "Error during backup: $_" "ERROR"
    Write-Log "Stack trace: $($_.ScriptStackTrace)" "ERROR"
    exit 1
}
