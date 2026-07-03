<#
.SYNOPSIS
    Deploy or roll back the Hawk Merchandising Portal API on IIS.

.DESCRIPTION
    Normal deploy: extract api-{Version}.zip, swap the current junction, smoke test,
    auto-rollback on failure, and prune old releases.

    Rollback: swap junction to the previous release (second newest folder).

.PARAMETER Version
    Release version tag (e.g. 1.2.3 or v1.2.3). Required unless -Rollback is set.

.PARAMETER Site
    IIS site name. Default: HawkPortal

.PARAMETER Rollback
    Roll back to the previous release without deploying a new version.

.EXAMPLE
    .\deploy.ps1 -Version "1.2.3"

.EXAMPLE
    .\deploy.ps1 -Rollback
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Version,

    [string]$Site = "HawkPortal",

    [switch]$Rollback
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Configuration — edit paths here for your environment
# ---------------------------------------------------------------------------
$SiteRoot          = "C:\Sites\HawkPortal"
$ReleasesPath      = Join-Path $SiteRoot "releases"
$CurrentLink       = Join-Path $SiteRoot "current"
$SharedConfigPath  = Join-Path $SiteRoot "shared\appsettings.Production.json"
$ArtifactsPath     = Join-Path $PSScriptRoot "artifacts"
$MaxRetainedReleases = 3
$ApiBaseUrl        = ""   # Leave empty to auto-detect from IIS bindings

# ---------------------------------------------------------------------------
# Validation
# ---------------------------------------------------------------------------
if (-not $Rollback -and [string]::IsNullOrWhiteSpace($Version)) {
    throw "-Version is required unless using -Rollback."
}

if (-not $Rollback) {
    $Version = $Version.Trim().TrimStart("v")
}

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host ">> $Message" -ForegroundColor Yellow
}

function Import-IisModule {
    if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
        Write-Warning "WebAdministration module not found. IIS site stop/start will be skipped."
        return $false
    }
    Import-Module WebAdministration -ErrorAction SilentlyContinue | Out-Null
    return $true
}

function Test-IisSiteExists {
    param([string]$SiteName)
    $iisReady = Import-IisModule
    if (-not $iisReady) { return $false }
    return $null -ne (Get-Website -Name $SiteName -ErrorAction SilentlyContinue)
}

function Stop-IisSiteSafely {
    param([string]$SiteName)
    if (-not (Test-IisSiteExists -SiteName $SiteName)) {
        Write-Warning "IIS site '$SiteName' not found. Skipping stop (first deploy?)."
        return
    }
    $site = Get-Website -Name $SiteName
    if ($site.State -eq "Started") {
        Stop-Website -Name $SiteName
        Write-Host "Stopped IIS site: $SiteName" -ForegroundColor Green
    }
}

function Start-IisSiteSafely {
    param([string]$SiteName)
    if (-not (Test-IisSiteExists -SiteName $SiteName)) {
        Write-Warning "IIS site '$SiteName' not found. Skipping start (first deploy?)."
        return
    }
    $site = Get-Website -Name $SiteName
    if ($site.State -ne "Started") {
        Start-Website -Name $SiteName
        Write-Host "Started IIS site: $SiteName" -ForegroundColor Green
    }
}

function Get-JunctionTargetPath {
    param([string]$JunctionPath)
    if (-not (Test-Path -LiteralPath $JunctionPath)) {
        return $null
    }
    try {
        return (Resolve-Path -LiteralPath $JunctionPath).Path
    }
    catch {
        return $null
    }
}

function Remove-DirectoryJunction {
    param([string]$JunctionPath)
    if (-not (Test-Path -LiteralPath $JunctionPath)) {
        return
    }
    $item = Get-Item -LiteralPath $JunctionPath -Force
    if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
        cmd /c rmdir "`"$JunctionPath`"" | Out-Null
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to remove junction: $JunctionPath"
        }
    }
    else {
        throw "Path exists but is not a junction: $JunctionPath"
    }
}

function New-DirectoryJunction {
    param(
        [string]$LinkPath,
        [string]$TargetPath
    )
    if (-not (Test-Path -LiteralPath $TargetPath)) {
        throw "Release target does not exist: $TargetPath"
    }
    $parent = Split-Path -Parent $LinkPath
    if (-not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    $output = cmd /c mklink /J "`"$LinkPath`"" "`"$TargetPath`""
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create junction '$LinkPath' -> '$TargetPath': $output"
    }
    Write-Host "Junction: $LinkPath -> $TargetPath" -ForegroundColor Green
}

function Get-ApiBaseUrl {
    param([string]$SiteName)

    if (-not [string]::IsNullOrWhiteSpace($ApiBaseUrl)) {
        return $ApiBaseUrl.TrimEnd("/")
    }

    if (-not (Import-IisModule)) {
        throw "ApiBaseUrl is not configured and IIS bindings could not be read. Set `$ApiBaseUrl at the top of deploy.ps1."
    }

    $binding = Get-WebBinding -Name $SiteName -Protocol "https" -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if (-not $binding) {
        $binding = Get-WebBinding -Name $SiteName -Protocol "http" -ErrorAction SilentlyContinue |
            Select-Object -First 1
    }
    if (-not $binding) {
        throw "No HTTP/HTTPS binding found for IIS site '$SiteName'. Set `$ApiBaseUrl manually."
    }

    $parts = $binding.bindingInformation -split ":"
    $port = $parts[1]
    $hostHeader = $parts[2]
    $protocol = $binding.protocol

    if ([string]::IsNullOrWhiteSpace($hostHeader)) {
        return "${protocol}://localhost:${port}"
    }
    return "${protocol}://${hostHeader}"
}

function Swap-CurrentRelease {
    param([string]$TargetReleasePath)

    Write-Step "Swapping current junction"
    Stop-IisSiteSafely -SiteName $Site
    Remove-DirectoryJunction -JunctionPath $CurrentLink
    New-DirectoryJunction -LinkPath $CurrentLink -TargetPath $TargetReleasePath
    Start-IisSiteSafely -SiteName $Site
}

function Get-ReleaseVersionFromPath {
    param([string]$ReleasePath)
    if ([string]::IsNullOrWhiteSpace($ReleasePath)) { return $null }
    return Split-Path -Leaf $ReleasePath
}

function Remove-OldReleases {
    param(
        [string]$ActiveReleasePath
    )
    if (-not (Test-Path -LiteralPath $ReleasesPath)) {
        return
    }

    $releases = Get-ChildItem -LiteralPath $ReleasesPath -Directory |
        Sort-Object LastWriteTime -Descending

    if ($releases.Count -le $MaxRetainedReleases) {
        return
    }

    $toRemove = $releases | Select-Object -Skip $MaxRetainedReleases
    foreach ($release in $toRemove) {
        if ($ActiveReleasePath -and $release.FullName -eq $ActiveReleasePath) {
            Write-Host "Keeping active release: $($release.Name)" -ForegroundColor DarkYellow
            continue
        }
        Write-Host "Removing old release: $($release.Name)" -ForegroundColor DarkGray
        Remove-Item -LiteralPath $release.FullName -Recurse -Force
    }
}

function Invoke-SmokeTest {
    $baseUrl = Get-ApiBaseUrl -SiteName $Site
    Write-Step "Running smoke test against $baseUrl"
    $smokeScript = Join-Path $PSScriptRoot "smoke-test.ps1"
    if (-not (Test-Path -LiteralPath $smokeScript)) {
        throw "smoke-test.ps1 not found at $smokeScript"
    }
    & $smokeScript -BaseUrl $baseUrl
    return $LASTEXITCODE -eq 0
}

function Get-PreviousReleasePath {
    $releases = Get-ChildItem -LiteralPath $ReleasesPath -Directory -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending

    if ($releases.Count -lt 2) {
        return $null
    }

    return $releases[1].FullName
}

# ---------------------------------------------------------------------------
# Rollback flow
# ---------------------------------------------------------------------------
if ($Rollback) {
    Write-Host "=== API Rollback ===" -ForegroundColor Cyan

    if (-not (Test-Path -LiteralPath $ReleasesPath)) {
        throw "No releases folder found at $ReleasesPath"
    }

    $previousRelease = Get-PreviousReleasePath
    if (-not $previousRelease) {
        throw "No previous release available for rollback (need at least 2 releases)."
    }

    $restoredVersion = Get-ReleaseVersionFromPath -ReleasePath $previousRelease
    Write-Host "Rolling back to: $restoredVersion ($previousRelease)" -ForegroundColor Yellow

    Swap-CurrentRelease -TargetReleasePath $previousRelease

    Write-Host ""
    Write-Host "Rollback complete. Active version: $restoredVersion" -ForegroundColor Green
    exit 0
}

# ---------------------------------------------------------------------------
# Normal deploy flow
# ---------------------------------------------------------------------------
Write-Host "=== API Deploy v$Version ===" -ForegroundColor Cyan

$zipName = "api-$Version.zip"
$zipPath = Join-Path $ArtifactsPath $zipName
if (-not (Test-Path -LiteralPath $zipPath)) {
    throw "Artifact not found: $zipPath`nDownload the GitHub Actions artifact and place it in $ArtifactsPath"
}

$releasePath = Join-Path $ReleasesPath $Version
if (Test-Path -LiteralPath $releasePath) {
    Write-Warning "Release folder already exists; re-deploy will overwrite: $releasePath"
    Remove-Item -LiteralPath $releasePath -Recurse -Force
}

Write-Step "Extracting $zipName to $releasePath"
New-Item -ItemType Directory -Path $releasePath -Force | Out-Null
Expand-Archive -LiteralPath $zipPath -DestinationPath $releasePath -Force

Write-Step "Copying shared production config"
if (-not (Test-Path -LiteralPath $SharedConfigPath)) {
    throw "Shared config not found: $SharedConfigPath`nCreate it before first deploy."
}
Copy-Item -LiteralPath $SharedConfigPath -Destination (Join-Path $releasePath "appsettings.Production.json") -Force
Write-Host "Copied appsettings.Production.json" -ForegroundColor Green

$previousReleasePath = Get-JunctionTargetPath -JunctionPath $CurrentLink
$previousVersion = Get-ReleaseVersionFromPath -ReleasePath $previousReleasePath
if ($previousVersion) {
    Write-Host "Current active version: $previousVersion" -ForegroundColor DarkCyan
}
else {
    Write-Host "No current junction found (first deploy)." -ForegroundColor DarkCyan
}

Swap-CurrentRelease -TargetReleasePath $releasePath

# Brief pause for IIS/app pool to come up
Start-Sleep -Seconds 3

$smokePassed = Invoke-SmokeTest
if (-not $smokePassed) {
    Write-Host ""
    Write-Host "Smoke test FAILED." -ForegroundColor Red

    if ($previousReleasePath -and (Test-Path -LiteralPath $previousReleasePath)) {
        Write-Host "Auto-rolling back to: $previousVersion" -ForegroundColor Yellow
        Swap-CurrentRelease -TargetReleasePath $previousReleasePath
        Start-Sleep -Seconds 3
        $rollbackSmoke = Invoke-SmokeTest
        if ($rollbackSmoke) {
            Write-Host "Auto-rollback succeeded. Active version: $previousVersion" -ForegroundColor Green
        }
        else {
            Write-Host "Auto-rollback completed but smoke test still failing. Manual intervention required." -ForegroundColor Red
        }
        exit 1
    }
    else {
        Write-Host "No previous release to roll back to. Manual intervention required." -ForegroundColor Red
        exit 1
    }
}

Write-Step "Pruning old releases (keeping $MaxRetainedReleases)"
$activePath = Get-JunctionTargetPath -JunctionPath $CurrentLink
Remove-OldReleases -ActiveReleasePath $activePath

Write-Host ""
Write-Host "Deploy complete. Active version: $Version" -ForegroundColor Green
exit 0
