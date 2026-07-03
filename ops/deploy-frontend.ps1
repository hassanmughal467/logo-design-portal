<#
.SYNOPSIS
    Deploy the Hawk Merchandising Portal Angular frontend on IIS.

.DESCRIPTION
    Copies (or extracts) frontend static files into a versioned release folder,
    then swaps the current junction. No smoke test is run for static assets.

.PARAMETER Version
    Release version tag (e.g. 1.2.3 or v1.2.3).

.PARAMETER DistPath
    Path to the Angular dist output folder. Use this when deploying from a local build.

.PARAMETER ZipPath
    Path to a frontend ZIP artifact. When set, extracts instead of copying DistPath.
    Defaults to artifacts\frontend-{Version}.zip if DistPath is not provided.

.PARAMETER Site
    IIS site name. Default: HawkPortalFrontend

.EXAMPLE
    .\deploy-frontend.ps1 -Version "1.2.3" -DistPath "C:\builds\dist\logo-design-portal-frontend"

.EXAMPLE
    .\deploy-frontend.ps1 -Version "1.2.3"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$DistPath = "",

    [string]$ZipPath = "",

    [string]$Site = "HawkPortalFrontend"
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Configuration — edit paths here for your environment
# ---------------------------------------------------------------------------
$SiteRoot          = "C:\Sites\HawkPortalFrontend"
$ReleasesPath      = Join-Path $SiteRoot "releases"
$CurrentLink       = Join-Path $SiteRoot "current"
$ArtifactsPath     = Join-Path $PSScriptRoot "artifacts"
$MaxRetainedReleases = 3

# ---------------------------------------------------------------------------
# Helpers (shared pattern with deploy.ps1)
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

function Remove-OldReleases {
    param([string]$ActiveReleasePath)
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

# ---------------------------------------------------------------------------
# Deploy
# ---------------------------------------------------------------------------
$Version = $Version.Trim().TrimStart("v")
Write-Host "=== Frontend Deploy v$Version ===" -ForegroundColor Cyan

$releasePath = Join-Path $ReleasesPath $Version
if (Test-Path -LiteralPath $releasePath) {
    Write-Warning "Release folder already exists; re-deploy will overwrite: $releasePath"
    Remove-Item -LiteralPath $releasePath -Recurse -Force
}
New-Item -ItemType Directory -Path $releasePath -Force | Out-Null

$resolvedZipPath = $ZipPath
if ([string]::IsNullOrWhiteSpace($resolvedZipPath) -and [string]::IsNullOrWhiteSpace($DistPath)) {
    $resolvedZipPath = Join-Path $ArtifactsPath "frontend-$Version.zip"
}

if (-not [string]::IsNullOrWhiteSpace($resolvedZipPath)) {
    if (-not (Test-Path -LiteralPath $resolvedZipPath)) {
        throw "Frontend ZIP not found: $resolvedZipPath`nProvide -DistPath or place frontend-$Version.zip in $ArtifactsPath"
    }
    Write-Step "Extracting $(Split-Path -Leaf $resolvedZipPath) to $releasePath"
    Expand-Archive -LiteralPath $resolvedZipPath -DestinationPath $releasePath -Force
}
else {
    if (-not (Test-Path -LiteralPath $DistPath)) {
        throw "Dist folder not found: $DistPath"
    }
    Write-Step "Copying dist from $DistPath to $releasePath"
    Copy-Item -LiteralPath (Join-Path $DistPath "*") -Destination $releasePath -Recurse -Force
}

Write-Step "Swapping current junction"
Stop-IisSiteSafely -SiteName $Site
Remove-DirectoryJunction -JunctionPath $CurrentLink
New-DirectoryJunction -LinkPath $CurrentLink -TargetPath $releasePath
Start-IisSiteSafely -SiteName $Site

Write-Step "Pruning old releases (keeping $MaxRetainedReleases)"
$activePath = Get-JunctionTargetPath -JunctionPath $CurrentLink
Remove-OldReleases -ActiveReleasePath $activePath

Write-Host ""
Write-Host "Frontend deploy complete. Active version: $Version" -ForegroundColor Green
exit 0
