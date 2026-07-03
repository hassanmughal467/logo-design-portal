# Logo Design Portal - Staging IIS Deployment
# Run from project root on the staging Windows/IIS host:
#   .\deploy-to-staging-iis.ps1
#
# Requires: .NET 8 SDK, Node 18+, IIS with ASP.NET Core Hosting Bundle
# See docs/STAGING_SETUP_GUIDE.md and docs/STAGING_DEPLOYMENT_CHECKLIST.md

$ErrorActionPreference = "Stop"
$DeployPath = "C:\inetpub\LogoDesignPortal-Staging"
$ApiPath = "$DeployPath\api"
$FrontendPath = "$DeployPath\wwwroot"
$ProjectRoot = $PSScriptRoot

Write-Host "=== Logo Design Portal - Staging IIS Deployment ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/6] Publishing Backend API (Release)..." -ForegroundColor Yellow
Push-Location "$ProjectRoot\Backend\src\LogoDesignPortal.API"
$env:ASPNETCORE_ENVIRONMENT = "Staging"
dotnet publish -c Release -o $ApiPath
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
Pop-Location
Write-Host "  Backend published to $ApiPath" -ForegroundColor Green

$LogsPath = "$ApiPath\logs"
if (-not (Test-Path $LogsPath)) {
    New-Item -ItemType Directory -Path $LogsPath -Force | Out-Null
}

$FilesStaging = "$ApiPath\Files_Staging"
if (-not (Test-Path $FilesStaging)) {
    New-Item -ItemType Directory -Path $FilesStaging -Force | Out-Null
    Write-Host "  Created Files_Staging: $FilesStaging" -ForegroundColor Green
}

Write-Host ""
Write-Host "[2/6] Building Frontend (staging configuration)..." -ForegroundColor Yellow
Push-Location "$ProjectRoot\Frontend"
npm ci
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
npm run build:staging
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
Pop-Location
Write-Host "  Frontend built (staging API URL baked in)" -ForegroundColor Green

Write-Host ""
Write-Host "[3/6] Copying Frontend to $FrontendPath..." -ForegroundColor Yellow
$FrontendDist = "$ProjectRoot\Frontend\dist\logo-design-portal-frontend"
if (-not (Test-Path $FrontendDist)) {
    Write-Host "  ERROR: Frontend dist not found at $FrontendDist" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $FrontendPath)) {
    New-Item -ItemType Directory -Path $FrontendPath -Force | Out-Null
}
Copy-Item "$FrontendDist\*" -Destination $FrontendPath -Recurse -Force
Copy-Item "$ProjectRoot\Frontend\web.config" -Destination $FrontendPath -Force
Write-Host "  Frontend copied" -ForegroundColor Green

Write-Host ""
Write-Host "[4/6] Staging web.config reminder..." -ForegroundColor Yellow
$StagingWebConfigExample = "$ProjectRoot\Backend\src\LogoDesignPortal.API\web.config.staging.example.xml"
Write-Host "  Copy and configure: $StagingWebConfigExample" -ForegroundColor Cyan
Write-Host "  -> $ApiPath\web.config (set ConnectionStrings, Jwt__Key, Redis; enable WebSockets)" -ForegroundColor Cyan

Write-Host ""
Write-Host "[5/6] Run EF migrations (if upgrading DB)..." -ForegroundColor Yellow
Write-Host "  Database:RunAfterStartup is false in Staging — apply migrations manually before or after deploy." -ForegroundColor DarkGray
Write-Host "  `$env:ASPNETCORE_ENVIRONMENT = 'Staging'" -ForegroundColor DarkGray
Write-Host "  Set ConnectionStrings__DefaultConnection, then:" -ForegroundColor DarkGray
Write-Host "  Backend/scripts/ApplyAllMigrations.sql (preferred) OR dotnet ef database update" -ForegroundColor DarkGray

Write-Host ""
Write-Host "[6/6] Staging deploy files ready!" -ForegroundColor Green
Write-Host ""
Write-Host "=== IIS sites (suggested) ===" -ForegroundColor Cyan
Write-Host "  API:      staging-api.hawkmerchandising.com  -> $ApiPath"
Write-Host "  Frontend: staging-admin.hawkmerchandising.com -> $FrontendPath"
Write-Host ""
Write-Host "Post-deploy: curl https://staging-api.hawkmerchandising.com/health/live" -ForegroundColor Cyan
Write-Host "See docs/STAGING_DEPLOYMENT_CHECKLIST.md and docs/deployment.md" -ForegroundColor Cyan
