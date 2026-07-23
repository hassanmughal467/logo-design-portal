<#
.SYNOPSIS
  Rapid pre-go-live test orchestration for Logo Design Portal.

.DESCRIPTION
  Runs backend unit/integration tests, optional Angular unit tests, and Playwright E2E
  in tiered profiles. Use before staging UAT and production releases.

.PARAMETER Tier
  quick    - Backend only (~2 min)
  standard - Backend + Playwright smoke/security (~10 min, API must be running)
  full     - Backend + Karma + full Playwright suite (~35 min)
  staging  - Staging health smoke (set STAGING_API_URL)

.PARAMETER StartServers
  Start local API (Development) and Angular (e2e) before E2E. Skipped for quick/staging.

.EXAMPLE
  .\scripts\run-pre-go-live-tests.ps1 -Tier quick

.EXAMPLE
  .\scripts\run-pre-go-live-tests.ps1 -Tier full -StartServers
#>
[CmdletBinding()]
param(
    [ValidateSet('quick', 'standard', 'full', 'staging')]
    [string] $Tier = 'standard',

    [switch] $StartServers
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$BackendRoot = Join-Path $RepoRoot 'Backend'
$FrontendRoot = Join-Path $RepoRoot 'Frontend'
$LogDir = Join-Path $RepoRoot 'test-results\pre-go-live'
$Timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$SummaryFile = Join-Path $LogDir "summary-$Timestamp.txt"

function Write-Step([string] $Message) {
    Write-Host "`n=== $Message ===" -ForegroundColor Cyan
}

function Ensure-Dir([string] $Path) {
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
}

function Invoke-Step {
    param(
        [string] $Name,
        [scriptblock] $Action
    )
    Write-Step $Name
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        & $Action
        $sw.Stop()
        $secs = [math]::Round($sw.Elapsed.TotalSeconds, 1)
        $line = "PASS  $Name (${secs}s)"
        Write-Host $line -ForegroundColor Green
        return @{ Name = $Name; Status = 'PASS'; Seconds = $sw.Elapsed.TotalSeconds }
    }
    catch {
        $sw.Stop()
        $secs = [math]::Round($sw.Elapsed.TotalSeconds, 1)
        $line = "FAIL  $Name (${secs}s) - $($_.Exception.Message)"
        Write-Host $line -ForegroundColor Red
        return @{ Name = $Name; Status = 'FAIL'; Seconds = $sw.Elapsed.TotalSeconds; Error = $_.Exception.Message }
    }
}

function Wait-HttpOk {
    param(
        [string] $Url,
        [int] $Attempts = 60,
        [int] $DelaySeconds = 2
    )
    for ($i = 1; $i -le $Attempts; $i++) {
        try {
            $r = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5 -SkipCertificateCheck
            if ($r.StatusCode -ge 200 -and $r.StatusCode -lt 400) { return }
        }
        catch { }
        Start-Sleep -Seconds $DelaySeconds
    }
    throw "Timed out waiting for $Url"
}

Ensure-Dir $LogDir
$results = [System.Collections.ArrayList]::new()

Write-Host @"

Logo Design Portal - Pre-Go-Live Test Runner
Tier: $Tier
Repo: $RepoRoot

"@ -ForegroundColor Yellow

# --- Backend (all tiers except staging-only) ---
if ($Tier -ne 'staging') {
    [void]$results.Add((Invoke-Step 'Backend - Domain + Application + Integration' {
        Push-Location $BackendRoot
        try {
            dotnet test LogoDesignPortal.sln -c Release --verbosity minimal
            if ($LASTEXITCODE -ne 0) { throw "dotnet test exited with code $LASTEXITCODE" }
        }
        finally { Pop-Location }
    }))
}

# --- Staging smoke ---
if ($Tier -eq 'staging') {
    if (-not $env:STAGING_API_URL) {
        throw 'Set STAGING_API_URL (e.g. https://staging-api.example.com) for staging tier.'
    }
    [void]$results.Add((Invoke-Step 'Staging - health live/ready' {
        Push-Location $FrontendRoot
        try {
            npx playwright test e2e/tests/smoke/staging-health.spec.ts --config=e2e/playwright.config.ts --project=chromium
            if ($LASTEXITCODE -ne 0) { throw "Playwright staging smoke exited with code $LASTEXITCODE" }
        }
        finally { Pop-Location }
    }))
}

# --- Optional server bootstrap for local E2E ---
$apiJob = $null
$ngJob = $null
if ($StartServers -and $Tier -in @('standard', 'full')) {
    Write-Step 'Starting local API (Development, https://localhost:44398)'
    $apiJob = Start-Job -ScriptBlock {
        param($Root)
        Set-Location $Root
        $env:ASPNETCORE_ENVIRONMENT = 'Development'
        dotnet run --project src/LogoDesignPortal.API/LogoDesignPortal.API.csproj -c Release --no-launch-profile --urls 'https://localhost:44398'
    } -ArgumentList $BackendRoot

    Write-Step 'Starting Angular (e2e config, http://localhost:4200)'
    $ngJob = Start-Job -ScriptBlock {
        param($Root)
        Set-Location $Root
        npx ng serve --configuration=e2e --port 4200
    } -ArgumentList $FrontendRoot

    Wait-HttpOk 'https://localhost:44398/health' | Out-Null
    Wait-HttpOk 'http://localhost:4200/' | Out-Null
    $env:E2E_REUSE_SERVER = 'true'
}

try {
    if ($Tier -eq 'full') {
        [void]$results.Add((Invoke-Step 'Frontend - Karma unit tests' {
            Push-Location $FrontendRoot
            try {
                if (-not $env:CHROME_BIN) {
                    $chrome = Get-Command chrome -ErrorAction SilentlyContinue
                    if ($chrome) { $env:CHROME_BIN = $chrome.Source }
                }
                npm run test:ci -- --browsers=ChromeHeadlessCI
                if ($LASTEXITCODE -ne 0) { throw "Karma exited with code $LASTEXITCODE" }
            }
            finally { Pop-Location }
        }))
    }

    if ($Tier -eq 'standard') {
        [void]$results.Add((Invoke-Step 'E2E - smoke + security + auth' {
            Push-Location $FrontendRoot
            try {
                $env:PW_WORKERS = '2'
                npx playwright test --config=e2e/playwright.config.ts `
                    e2e/tests/smoke/critical-paths.spec.ts `
                    e2e/tests/auth/login.spec.ts `
                    e2e/tests/auth/logout.spec.ts `
                    e2e/tests/access/unauthorized.spec.ts `
                    e2e/tests/security/rbac-and-token.spec.ts `
                    e2e/tests/workflows/order-lifecycle.e2e.spec.ts `
                    e2e/tests/elite/full-business-lifecycle.spec.ts
                if ($LASTEXITCODE -ne 0) { throw "Playwright smoke exited with code $LASTEXITCODE" }
            }
            finally { Pop-Location }
        }))
    }

    if ($Tier -eq 'full') {
        [void]$results.Add((Invoke-Step 'E2E - full Playwright suite (elite serial first)' {
            Push-Location $FrontendRoot
            try {
                # Serial elite tests avoid login/API contention; then parallel remainder.
                npx playwright test --config=e2e/playwright.config.ts --project=elite-serial
                if ($LASTEXITCODE -ne 0) { throw "Playwright elite-serial exited with code $LASTEXITCODE" }

                $env:PW_WORKERS = '3'
                npx playwright test --config=e2e/playwright.config.ts --project=chromium --project=admin-chromium
                if ($LASTEXITCODE -ne 0) { throw "Playwright full suite exited with code $LASTEXITCODE" }
            }
            finally { Pop-Location }
        }))
    }
}
finally {
    if ($apiJob) { Stop-Job $apiJob -ErrorAction SilentlyContinue; Remove-Job $apiJob -Force -ErrorAction SilentlyContinue }
    if ($ngJob) { Stop-Job $ngJob -ErrorAction SilentlyContinue; Remove-Job $ngJob -Force -ErrorAction SilentlyContinue }
}

# --- Summary ---
$passed = @($results | Where-Object { $_.Status -eq 'PASS' }).Count
$failed = @($results | Where-Object { $_.Status -eq 'FAIL' }).Count
$totalSeconds = 0.0
foreach ($r in $results) { $totalSeconds += $r.Seconds }
$totalSeconds = [math]::Round($totalSeconds, 1)

$durationLabel = "${totalSeconds}s total"
$summary = @(
    "Pre-Go-Live Summary - $Timestamp",
    "Tier: $Tier",
    "Steps: $passed passed, $failed failed ($durationLabel)",
    ''
)
foreach ($r in $results) {
    $summary += "$($r.Status)  $($r.Name)"
    if ($r.Error) { $summary += "       $($r.Error)" }
}
$summaryText = $summary -join "`n"
$summaryText | Set-Content -Path $SummaryFile -Encoding UTF8

Write-Host "`n$summaryText`n" -ForegroundColor $(if ($failed -eq 0) { 'Green' } else { 'Red' })
Write-Host "Summary saved: $SummaryFile" -ForegroundColor DarkGray

if ($failed -gt 0) { exit 1 }
exit 0
