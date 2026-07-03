<#
.SYNOPSIS
    Daily MySQL backup for Hawk Merchandising Portal — dump, compress, upload to R2.

.DESCRIPTION
    Runs mysqldump with a consistent snapshot (--single-transaction), compresses
    the output, uploads to Cloudflare R2 (hawk-backups/daily/), prunes old local
    and remote backups, and logs results to C:\Logs\backup.log.

    Required environment variables:
      MYSQL_ROOT_PASSWORD, CF_ACCOUNT_ID, CF_R2_ACCESS_KEY, CF_R2_SECRET_KEY

    Exit code 0 on success, 1 on failure (for Task Scheduler).

.EXAMPLE
    .\backup-mysql.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Configuration — edit paths here for your environment
# ---------------------------------------------------------------------------
$MySqlDumpPath     = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe"
$DatabaseName      = "LogoDesignPortalDb"
$LocalBackupDir    = "C:\Backups\MySQL"
$LogFile           = "C:\Logs\backup.log"
$LocalRetentionDays = 7
$R2Bucket          = "hawk-backups"
$R2DailyPrefix     = "daily"
$R2RetentionCount  = 30

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
function Write-BackupLog {
    param(
        [string]$Message,
        [ValidateSet("INFO", "ERROR")]
        [string]$Level = "INFO"
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$timestamp] [$Level] $Message"

    $logDir = Split-Path -Parent $LogFile
    if (-not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }

    Add-Content -Path $LogFile -Value $line -Encoding UTF8

    if ($Level -eq "ERROR") {
        Write-Host $line -ForegroundColor Red
    }
    else {
        Write-Host $line -ForegroundColor Green
    }
}

function Exit-BackupFailure {
    param([string]$Message)
    Write-BackupLog -Message $Message -Level "ERROR"
    exit 1
}

function Test-RequiredEnv {
    param([string[]]$Names)

    $missing = @()
    foreach ($name in $Names) {
        $value = [Environment]::GetEnvironmentVariable($name)
        if ([string]::IsNullOrWhiteSpace($value)) {
            $missing += $name
        }
    }

    if ($missing.Count -gt 0) {
        Exit-BackupFailure "Missing required environment variables: $($missing -join ', ')"
    }
}

function Get-SevenZipPath {
    $candidates = @(
        "${env:ProgramFiles}\7-Zip\7z.exe",
        "${env:ProgramFiles(x86)}\7-Zip\7z.exe"
    )

    foreach ($path in $candidates) {
        if (Test-Path $path) { return $path }
    }

    return $null
}

function Compress-FileToGzip {
    param(
        [string]$SourceFile,
        [string]$DestinationFile
    )

    $sevenZip = Get-SevenZipPath
    if ($sevenZip) {
        & $sevenZip a -tgzip $DestinationFile $SourceFile | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "7-Zip compression failed with exit code $LASTEXITCODE"
        }
        return
    }

    $inputStream  = [System.IO.File]::OpenRead($SourceFile)
    $outputStream = [System.IO.File]::Create($DestinationFile)
    $gzipStream   = New-Object System.IO.Compression.GZipStream($outputStream, [System.IO.Compression.CompressionMode]::Compress)

    try {
        $inputStream.CopyTo($gzipStream)
    }
    finally {
        $gzipStream.Dispose()
        $outputStream.Dispose()
        $inputStream.Dispose()
    }
}

function Remove-OldLocalBackups {
    param(
        [string]$Directory,
        [int]$RetentionDays
    )

    if (-not (Test-Path $Directory)) { return 0 }

    $cutoff = (Get-Date).AddDays(-$RetentionDays)
    $removed = 0

    Get-ChildItem -Path $Directory -File -Filter "${DatabaseName}-*.sql.gz" |
        Where-Object { $_.LastWriteTime -lt $cutoff } |
        ForEach-Object {
            Remove-Item -Path $_.FullName -Force
            $removed++
            Write-BackupLog "Deleted local backup older than $RetentionDays days: $($_.Name)"
        }

    return $removed
}

function Remove-OldR2Backups {
    param(
        [int]$KeepCount
    )

    $endpoint = "https://$($env:CF_ACCOUNT_ID).r2.cloudflarestorage.com"
    $listOutput = aws s3 ls "s3://$R2Bucket/$R2DailyPrefix/" --endpoint-url $endpoint 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to list R2 backups: $listOutput"
    }

    $objects = @()
    foreach ($line in ($listOutput -split "`n")) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }

        # aws s3 ls format: "2026-06-10 02:00:15    1234567 LogoDesignPortalDb-....sql.gz"
        if ($line -match '^\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\s+\d+\s+(.+)$') {
            $objects += [PSCustomObject]@{
                Key          = "$R2DailyPrefix/$($Matches[1].Trim())"
                FileName     = $Matches[1].Trim()
                LastModified = [datetime]::ParseExact($line.Substring(0, 19), "yyyy-MM-dd HH:mm:ss", $null)
            }
        }
    }

    if ($objects.Count -le $KeepCount) { return 0 }

    $toDelete = $objects |
        Sort-Object LastModified -Descending |
        Select-Object -Skip $KeepCount

    $deleted = 0
    foreach ($obj in $toDelete) {
        $removeOutput = aws s3 rm "s3://$R2Bucket/$($obj.Key)" --endpoint-url $endpoint 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to delete R2 object $($obj.Key): $removeOutput"
        }
        $deleted++
        Write-BackupLog "Deleted R2 backup beyond retention ($KeepCount kept): $($obj.FileName)"
    }

    return $deleted
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
try {
    Write-BackupLog "Starting MySQL backup for database '$DatabaseName'"

    Test-RequiredEnv -Names @(
        "MYSQL_ROOT_PASSWORD",
        "CF_ACCOUNT_ID",
        "CF_R2_ACCESS_KEY",
        "CF_R2_SECRET_KEY"
    )

    if (-not (Test-Path $MySqlDumpPath)) {
        Exit-BackupFailure "mysqldump not found at: $MySqlDumpPath"
    }

    if (-not (Test-Path $LocalBackupDir)) {
        New-Item -ItemType Directory -Path $LocalBackupDir -Force | Out-Null
        Write-BackupLog "Created local backup directory: $LocalBackupDir"
    }

    $timestamp   = Get-Date -Format "yyyy-MM-dd-HHmm"
    $baseName    = "$DatabaseName-$timestamp"
    $sqlFile     = Join-Path $LocalBackupDir "$baseName.sql"
    $gzipFile    = Join-Path $LocalBackupDir "$baseName.sql.gz"

    # Avoid overwriting if run twice in the same minute (idempotent re-run)
    if (Test-Path $gzipFile) {
        Write-BackupLog "Backup file already exists for this minute; skipping dump: $baseName.sql.gz"
        exit 0
    }

    Write-BackupLog "Running mysqldump -> $sqlFile"

    $dumpArgs = @(
        "--host=localhost",
        "--user=root",
        "--password=$($env:MYSQL_ROOT_PASSWORD)",
        "--single-transaction",
        "--routines",
        "--triggers",
        "--events",
        "--result-file=$sqlFile",
        $DatabaseName
    )

    & $MySqlDumpPath @dumpArgs
    if ($LASTEXITCODE -ne 0) {
        if (Test-Path $sqlFile) { Remove-Item $sqlFile -Force }
        Exit-BackupFailure "mysqldump failed with exit code $LASTEXITCODE"
    }

    if (-not (Test-Path $sqlFile) -or (Get-Item $sqlFile).Length -eq 0) {
        Exit-BackupFailure "mysqldump produced an empty or missing SQL file"
    }

    $sqlSizeMb = [math]::Round((Get-Item $sqlFile).Length / 1MB, 2)
    Write-BackupLog "mysqldump complete ($sqlSizeMb MB uncompressed)"

    Write-BackupLog "Compressing -> $gzipFile"
    Compress-FileToGzip -SourceFile $sqlFile -DestinationFile $gzipFile
    Remove-Item $sqlFile -Force

    $gzipSizeMb = [math]::Round((Get-Item $gzipFile).Length / 1MB, 2)
    Write-BackupLog "Compression complete ($gzipSizeMb MB)"

    $env:AWS_ACCESS_KEY_ID     = $env:CF_R2_ACCESS_KEY
    $env:AWS_SECRET_ACCESS_KEY = $env:CF_R2_SECRET_KEY
    $r2Endpoint = "https://$($env:CF_ACCOUNT_ID).r2.cloudflarestorage.com"
    $r2Key      = "$R2DailyPrefix/$baseName.sql.gz"

    Write-BackupLog "Uploading to s3://$R2Bucket/$r2Key"
    $uploadOutput = aws s3 cp $gzipFile "s3://$R2Bucket/$r2Key" --endpoint-url $r2Endpoint 2>&1
    if ($LASTEXITCODE -ne 0) {
        Exit-BackupFailure "R2 upload failed: $uploadOutput"
    }

    Write-BackupLog "Upload complete"

    $localRemoved = Remove-OldLocalBackups -Directory $LocalBackupDir -RetentionDays $LocalRetentionDays
    Write-BackupLog "Local retention: removed $localRemoved file(s) older than $LocalRetentionDays days"

    $r2Removed = Remove-OldR2Backups -KeepCount $R2RetentionCount
    Write-BackupLog "R2 retention: removed $r2Removed file(s); keeping newest $R2RetentionCount daily backups"

    Write-BackupLog "Backup succeeded: $baseName.sql.gz"
    exit 0
}
catch {
    Exit-BackupFailure "Unhandled error: $($_.Exception.Message)"
}
