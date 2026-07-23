<#
.SYNOPSIS
    Restore a compressed MySQL backup for Hawk Merchandising Portal.

.DESCRIPTION
    Decompresses a .sql.gz backup, optionally creates the target database,
    imports the dump, and verifies row counts in key tables.

    Required environment variable: MYSQL_ROOT_PASSWORD

.PARAMETER BackupFile
    Path to the .sql.gz backup file.

.PARAMETER TargetDatabase
    Database to restore into. Default: LogoDesignPortalDb

.PARAMETER CreateDatabase
    Create the target database if it does not exist.

.PARAMETER MySqlPath
    Path to mysql.exe. Default: C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe

.EXAMPLE
    .\restore-mysql.ps1 -BackupFile "C:\Backups\MySQL\LogoDesignPortalDb-2026-06-10-0200.sql.gz"

.EXAMPLE
    .\restore-mysql.ps1 -BackupFile ".\backup.sql.gz" -TargetDatabase "LogoDesignPortalDb_Test" -CreateDatabase
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$BackupFile,

    [string]$TargetDatabase = "LogoDesignPortalDb",

    [switch]$CreateDatabase,

    [string]$MySqlPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Configuration
# ---------------------------------------------------------------------------
$TempDir = Join-Path $env:TEMP "mysql-restore-$(Get-Date -Format 'yyyyMMddHHmmss')"

# Key tables — LogoOrders is the physical table name for orders
$VerifyTables = @(
    @{ DisplayName = "Orders";   TableName = "LogoOrders" },
    @{ DisplayName = "Invoices"; TableName = "Invoices" },
    @{ DisplayName = "Users";    TableName = "Users" }
)

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
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

function Expand-GzipFile {
    param(
        [string]$GzipFile,
        [string]$OutputFile
    )

    $sevenZip = Get-SevenZipPath
    if ($sevenZip) {
        & $sevenZip x $GzipFile "-o$(Split-Path -Parent $OutputFile)" -y | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "7-Zip decompression failed with exit code $LASTEXITCODE"
        }

        $extracted = Get-ChildItem -Path (Split-Path -Parent $OutputFile) -Filter "*.sql" |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1

        if (-not $extracted) {
            throw "7-Zip did not produce a .sql file"
        }

        if ($extracted.FullName -ne $OutputFile) {
            Move-Item -Path $extracted.FullName -Destination $OutputFile -Force
        }

        return
    }

    $inputStream  = [System.IO.File]::OpenRead($GzipFile)
    $outputStream = [System.IO.File]::Create($OutputFile)
    $gzipStream   = New-Object System.IO.Compression.GZipStream($inputStream, [System.IO.Compression.CompressionMode]::Decompress)

    try {
        $gzipStream.CopyTo($outputStream)
    }
    finally {
        $gzipStream.Dispose()
        $outputStream.Dispose()
        $inputStream.Dispose()
    }
}

function Invoke-MySqlQuery {
    param([string]$Arguments)

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName               = $MySqlPath
    $psi.Arguments              = $Arguments
    $psi.UseShellExecute        = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError  = $true
    $psi.CreateNoWindow         = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi
    $process.Start() | Out-Null

    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        throw "mysql failed (exit $($process.ExitCode)): $stderr"
    }

    return $stdout.Trim()
}

function Invoke-MySqlImport {
    param(
        [string]$Arguments,
        [string]$SqlFile
    )

    # cmd.exe input redirection handles large dumps reliably on Windows
    $importCmd = "`"$MySqlPath`" $Arguments < `"$SqlFile`""
    cmd.exe /c $importCmd 2>&1 | ForEach-Object { $_ }

    if ($LASTEXITCODE -ne 0) {
        throw "mysql import failed with exit code $LASTEXITCODE"
    }
}

function Test-DatabaseExists {
    param([string]$DatabaseName)

    $args = @(
        "--host=localhost",
        "--user=root",
        "--password=$($env:MYSQL_ROOT_PASSWORD)",
        "-N",
        "-e", "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '$DatabaseName';"
    ) -join " "
    $output = Invoke-MySqlQuery -Arguments $args

    return -not [string]::IsNullOrWhiteSpace($output)
}

function Get-TableRowCount {
    param(
        [string]$DatabaseName,
        [string]$TableName
    )

    $query = "SELECT COUNT(*) FROM ``$TableName``;"
    $args = @(
        "--host=localhost",
        "--user=root",
        "--password=$($env:MYSQL_ROOT_PASSWORD)",
        $DatabaseName,
        "-N",
        "-e", $query
    ) -join " "
    $output = Invoke-MySqlQuery -Arguments $args

    return [long]$output
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

try {
    Write-Host ""
    Write-Host ">> MySQL Restore" -ForegroundColor Yellow
    Write-Host "   Backup file : $BackupFile"
    Write-Host "   Target DB   : $TargetDatabase"
    Write-Host ""

    if ([string]::IsNullOrWhiteSpace($env:MYSQL_ROOT_PASSWORD)) {
        throw "Missing required environment variable: MYSQL_ROOT_PASSWORD"
    }

    if (-not (Test-Path $MySqlPath)) {
        throw "mysql client not found at: $MySqlPath"
    }

    $resolvedBackup = Resolve-Path -Path $BackupFile -ErrorAction Stop
    if ($resolvedBackup.Path -notmatch '\.sql(\.gz)?$') {
        throw "Backup file must be a .sql.gz or .sql file"
    }

    New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

    $sqlFile = Join-Path $TempDir "$TargetDatabase-restore.sql"

    if ($resolvedBackup.Path -match '\.sql\.gz$') {
        Write-Host ">> Decompressing backup..." -ForegroundColor Yellow
        Expand-GzipFile -GzipFile $resolvedBackup.Path -OutputFile $sqlFile
    }
    else {
        Copy-Item -Path $resolvedBackup.Path -Destination $sqlFile
    }

    if (-not (Test-Path $sqlFile) -or (Get-Item $sqlFile).Length -eq 0) {
        throw "Decompressed SQL file is missing or empty"
    }

    $sqlSizeMb = [math]::Round((Get-Item $sqlFile).Length / 1MB, 2)
    Write-Host "   Decompressed size: $sqlSizeMb MB"

    $dbExists = Test-DatabaseExists -DatabaseName $TargetDatabase

    if (-not $dbExists) {
        if (-not $CreateDatabase) {
            throw "Target database '$TargetDatabase' does not exist. Re-run with -CreateDatabase to create it."
        }

        Write-Host ">> Creating database '$TargetDatabase'..." -ForegroundColor Yellow
        $createArgs = @(
            "--host=localhost",
            "--user=root",
            "--password=$($env:MYSQL_ROOT_PASSWORD)",
            "-e", "CREATE DATABASE IF NOT EXISTS ``$TargetDatabase`` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
        ) -join " "
        Invoke-MySqlQuery -Arguments $createArgs | Out-Null
    }
    else {
        Write-Host ">> Target database '$TargetDatabase' already exists" -ForegroundColor Yellow
    }

    Write-Host ">> Importing SQL dump (this may take several minutes)..." -ForegroundColor Yellow

    $importArgs = @(
        "--host=localhost",
        "--user=root",
        "--password=$($env:MYSQL_ROOT_PASSWORD)",
        "--default-character-set=utf8mb4",
        $TargetDatabase
    ) -join " "
    Invoke-MySqlImport -Arguments $importArgs -SqlFile $sqlFile

    Write-Host ">> Verifying row counts..." -ForegroundColor Yellow

    $verification = @()
    foreach ($table in $VerifyTables) {
        try {
            $count = Get-TableRowCount -DatabaseName $TargetDatabase -TableName $table.TableName
            $verification += [PSCustomObject]@{
                Table     = $table.DisplayName
                TableName = $table.TableName
                RowCount  = $count
                Status    = "OK"
            }
        }
        catch {
            $verification += [PSCustomObject]@{
                Table     = $table.DisplayName
                TableName = $table.TableName
                RowCount  = $null
                Status    = "MISSING or ERROR: $($_.Exception.Message)"
            }
        }
    }

    $stopwatch.Stop()

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " RESTORE SUMMARY" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " Backup file    : $($resolvedBackup.Path)"
    Write-Host " Target database: $TargetDatabase"
    Write-Host " SQL size       : $sqlSizeMb MB"
    Write-Host " Duration       : $([math]::Round($stopwatch.Elapsed.TotalSeconds, 1)) seconds"
    Write-Host ""
    Write-Host " Table verification:"
    $verification | Format-Table -AutoSize | Out-String | Write-Host

    $failed = $verification | Where-Object { $_.Status -ne "OK" }
    if ($failed) {
        Write-Host "Restore completed with verification warnings." -ForegroundColor Yellow
        exit 1
    }

    Write-Host "Restore completed successfully." -ForegroundColor Green
    exit 0
}
catch {
    Write-Host ""
    Write-Host "RESTORE FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    if (Test-Path $TempDir) {
        Remove-Item -Path $TempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
