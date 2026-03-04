# Logo Design Portal - IIS Deployment Script
# Run from project root: .\deploy-to-iis.ps1

$ErrorActionPreference = "Stop"
$DeployPath = "C:\inetpub\LogoDesignPortal"
$ApiPath = "$DeployPath\api"
$FrontendPath = "$DeployPath\wwwroot"
$ProjectRoot = $PSScriptRoot

Write-Host "=== Logo Design Portal - IIS Deployment ===" -ForegroundColor Cyan
Write-Host ""

# 1. Publish Backend
Write-Host "[1/5] Publishing Backend API..." -ForegroundColor Yellow
Push-Location "$ProjectRoot\Backend\src\LogoDesignPortal.API"
dotnet publish -c Release -o $ApiPath
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
Pop-Location
Write-Host "  Backend published to $ApiPath" -ForegroundColor Green

# 2. Create logs folder for API
$LogsPath = "$ApiPath\logs"
if (-not (Test-Path $LogsPath)) {
    New-Item -ItemType Directory -Path $LogsPath -Force | Out-Null
    Write-Host "  Created logs folder: $LogsPath" -ForegroundColor Green
}

# 3. Build Frontend
Write-Host ""
Write-Host "[2/5] Building Frontend..." -ForegroundColor Yellow
Push-Location "$ProjectRoot\Frontend"
npm run build
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
Pop-Location
Write-Host "  Frontend built successfully" -ForegroundColor Green

# 4. Copy Frontend to deploy folder
Write-Host ""
Write-Host "[3/5] Copying Frontend to $FrontendPath..." -ForegroundColor Yellow
$FrontendDist = "$ProjectRoot\Frontend\dist\logo-design-portal-frontend"
if (-not (Test-Path $FrontendDist)) {
    Write-Host "  ERROR: Frontend dist folder not found at $FrontendDist" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $FrontendPath)) {
    New-Item -ItemType Directory -Path $FrontendPath -Force | Out-Null
}
Copy-Item "$FrontendDist\*" -Destination $FrontendPath -Recurse -Force
Write-Host "  Frontend copied" -ForegroundColor Green

# 5. Copy web.config for Angular routing (URL rewrite)
Write-Host ""
Write-Host "[4/5] Copying Angular web.config..." -ForegroundColor Yellow
Copy-Item "$ProjectRoot\Frontend\web.config" -Destination $FrontendPath -Force
Write-Host "  web.config copied" -ForegroundColor Green

# 6. Create Files folder for uploads if not exists
$FilesPath = "$ApiPath\Files"
if (-not (Test-Path $FilesPath)) {
    New-Item -ItemType Directory -Path $FilesPath -Force | Out-Null
    Write-Host "  Created Files folder for uploads" -ForegroundColor Green
}

Write-Host ""
Write-Host "[5/5] Deployment complete!" -ForegroundColor Green
Write-Host ""
Write-Host "=== NEXT STEPS (run in IIS Manager) ===" -ForegroundColor Cyan
Write-Host "1. Open IIS Manager (Win+R -> inetmgr)"
Write-Host "2. Add API Site:"
Write-Host "   - Right-click Sites -> Add Website"
Write-Host "   - Site name: LogoDesignPortal-API"
Write-Host "   - Physical path: $ApiPath"
Write-Host "   - Binding: http, Port 5000 (or any free port)"
Write-Host "   - Application Pool: Create new, set .NET CLR = No Managed Code"
Write-Host ""
Write-Host "3. Add Frontend Site:"
Write-Host "   - Right-click Sites -> Add Website"
Write-Host "   - Site name: LogoDesignPortal-Frontend"
Write-Host "   - Physical path: $FrontendPath"
Write-Host "   - Binding: http, Port 8080 (or 80 if available)"
Write-Host ""
Write-Host "4. Edit appsettings.Production.json in $ApiPath"
Write-Host "   - Set your MySQL connection string (Server, Database, User, Password)"
Write-Host ""
Write-Host "5. Test: http://localhost:5000/swagger (API) and http://localhost:8080 (Frontend)"
Write-Host ""
