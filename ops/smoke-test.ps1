<#
.SYNOPSIS
    Post-deploy smoke tests for the Hawk Merchandising Portal API.

.DESCRIPTION
    Verifies /health/live, /health/ready, and a public API endpoint.
    Exits 0 on PASS, 1 on FAIL. Compatible with PowerShell 5.1+.

.PARAMETER BaseUrl
    API base URL (no trailing slash). Example: https://api.hawkportal.example.com

.EXAMPLE
    .\smoke-test.ps1 -BaseUrl "https://api.hawkportal.example.com"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$BaseUrl
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Configuration — edit these defaults if needed
# ---------------------------------------------------------------------------
$LiveEndpoint   = "/health/live"
$ReadyEndpoint  = "/health/ready"
$PublicEndpoint = "/api/system/health"   # AllowAnonymous; no /api/auth/ping exists
$RequestTimeoutSec = 30

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------
function Invoke-SmokeRequest {
    param(
        [string]$Label,
        [string]$Method,
        [string]$Url,
        [scriptblock]$Assert
    )

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $params = @{
            Uri             = $Url
            Method          = $Method
            UseBasicParsing = $true
            TimeoutSec      = $RequestTimeoutSec
        }

        if ($Method -eq "POST") {
            $params["ContentType"] = "application/json"
            $params["Body"]        = "{}"
        }

        $response = Invoke-WebRequest @params
        $sw.Stop()
        $elapsedMs = $sw.ElapsedMilliseconds

        & $Assert $response

        return [PSCustomObject]@{
            Label      = $Label
            Status     = "PASS"
            StatusCode = $response.StatusCode
            ElapsedMs  = $elapsedMs
        }
    }
    catch {
        $sw.Stop()
        $statusCode = $null
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        return [PSCustomObject]@{
            Label      = $Label
            Status     = "FAIL"
            StatusCode = $statusCode
            ElapsedMs  = $sw.ElapsedMilliseconds
            Error      = $_.Exception.Message
        }
    }
}

# ---------------------------------------------------------------------------
# Run checks
# ---------------------------------------------------------------------------
$BaseUrl = $BaseUrl.TrimEnd("/")
Write-Host "=== Smoke Test: $BaseUrl ===" -ForegroundColor Cyan
Write-Host ""

$results = @()

$results += Invoke-SmokeRequest -Label "GET $LiveEndpoint" -Method "GET" `
    -Url "$BaseUrl$LiveEndpoint" `
    -Assert {
        param($r)
        if ($r.StatusCode -ne 200) {
            throw "Expected HTTP 200, got $($r.StatusCode)"
        }
    }

$results += Invoke-SmokeRequest -Label "GET $ReadyEndpoint" -Method "GET" `
    -Url "$BaseUrl$ReadyEndpoint" `
    -Assert {
        param($r)
        if ($r.StatusCode -ne 200) {
            throw "Expected HTTP 200, got $($r.StatusCode)"
        }
    }

$results += Invoke-SmokeRequest -Label "GET $PublicEndpoint" -Method "GET" `
    -Url "$BaseUrl$PublicEndpoint" `
    -Assert {
        param($r)
        if ($r.StatusCode -ge 500) {
            throw "Expected non-5xx, got $($r.StatusCode)"
        }
    }

# ---------------------------------------------------------------------------
# Report
# ---------------------------------------------------------------------------
$allPassed = $true
foreach ($result in $results) {
    $color = if ($result.Status -eq "PASS") { "Green" } else { "Red" }
    $codeDisplay = if ($null -ne $result.StatusCode) { $result.StatusCode } else { "n/a" }
    Write-Host ("{0,-35} {1,-6} HTTP {2,-4} {3,5} ms" -f $result.Label, $result.Status, $codeDisplay, $result.ElapsedMs) -ForegroundColor $color
    if ($result.Error) {
        Write-Host "  -> $($result.Error)" -ForegroundColor Red
    }
    if ($result.Status -ne "PASS") {
        $allPassed = $false
    }
}

Write-Host ""
if ($allPassed) {
    Write-Host "RESULT: PASS" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "RESULT: FAIL" -ForegroundColor Red
    exit 1
}
