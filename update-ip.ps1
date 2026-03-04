# Script to update IP address in configuration files
# Usage: .\update-ip.ps1 [IP_ADDRESS]
# If no IP provided, will detect automatically

param(
    [string]$NewIP = ""
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  IP Address Update Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get IP address
if ($NewIP -eq "") {
    Write-Host "Detecting your IP address..." -ForegroundColor Yellow
    $ipAddresses = Get-NetIPAddress -AddressFamily IPv4 | Where-Object {
        $_.InterfaceAlias -notlike "*Loopback*" -and 
        $_.IPAddress -notlike "169.254.*" -and
        $_.IPAddress -notlike "127.*"
    } | Sort-Object IPAddress
    
    if ($ipAddresses.Count -eq 0) {
        Write-Host "ERROR: Could not detect IP address." -ForegroundColor Red
        Write-Host "Please provide IP address manually: .\update-ip.ps1 192.168.1.100" -ForegroundColor Yellow
        exit 1
    }
    
    $NewIP = $ipAddresses[0].IPAddress
    Write-Host "Detected IP: $NewIP" -ForegroundColor Green
} else {
    Write-Host "Using provided IP: $NewIP" -ForegroundColor Green
}

Write-Host ""

# Update Backend Program.cs
$programCsPath = "Backend\src\LogoDesignPortal.API\Program.cs"
if (Test-Path $programCsPath) {
    Write-Host "Updating Backend CORS..." -ForegroundColor Yellow
    $content = Get-Content $programCsPath -Raw
    
    # Replace IP in CORS configuration
    $pattern = 'http://\d+\.\d+\.\d+\.\d+:4200'
    $replacement = "http://$NewIP:4200"
    
    if ($content -match $pattern) {
        $content = $content -replace $pattern, $replacement
        Set-Content $programCsPath -Value $content -NoNewline
        Write-Host "✓ Updated: $programCsPath" -ForegroundColor Green
    } else {
        Write-Host "⚠ Could not find IP pattern in Program.cs" -ForegroundColor Yellow
        Write-Host "  Please update manually around line 94" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠ File not found: $programCsPath" -ForegroundColor Yellow
}

# Update Frontend environment.ts
$envTsPath = "Frontend\src\environments\environment.ts"
if (Test-Path $envTsPath) {
    Write-Host "Updating Frontend environment..." -ForegroundColor Yellow
    $content = Get-Content $envTsPath -Raw
    
    # Replace IP in apiUrl
    $pattern = "apiUrl: 'http://\d+\.\d+\.\d+\.\d+:5000'"
    $replacement = "apiUrl: 'http://$NewIP:5000'"
    
    if ($content -match $pattern) {
        $content = $content -replace $pattern, $replacement
        Set-Content $envTsPath -Value $content -NoNewline
        Write-Host "✓ Updated: $envTsPath" -ForegroundColor Green
    } else {
        Write-Host "⚠ Could not find IP pattern in environment.ts" -ForegroundColor Yellow
        Write-Host "  Please update manually on line 3" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠ File not found: $envTsPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Update Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Restart backend server" -ForegroundColor White
Write-Host "2. Restart frontend server" -ForegroundColor White
Write-Host "3. Share new URL: http://$NewIP:4200" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to close..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
