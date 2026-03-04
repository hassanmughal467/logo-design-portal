# Multi-User Network Testing Setup Script
# This script helps configure your web portal for local network testing

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Multi-User Network Testing Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get local IP address
Write-Host "Finding your local IP address..." -ForegroundColor Yellow
$networkAdapters = Get-NetIPAddress -AddressFamily IPv4 | Where-Object {
    $_.InterfaceAlias -notlike "*Loopback*" -and 
    $_.IPAddress -notlike "169.254.*" -and
    $_.IPAddress -notlike "127.*"
} | Sort-Object IPAddress

if ($networkAdapters.Count -eq 0) {
    Write-Host "ERROR: Could not find network adapter. Please check your network connection." -ForegroundColor Red
    exit 1
}

$localIP = $networkAdapters[0].IPAddress
Write-Host "✓ Found local IP: $localIP" -ForegroundColor Green
Write-Host ""

# Create network environment file
Write-Host "Creating network environment file..." -ForegroundColor Yellow
$envContent = @"
export const environment = {
  production: false,
  apiUrl: 'http://$localIP:5000',
  apiVersion: ''
};
"@

$envFilePath = "Frontend\src\environments\environment.network.ts"
$envContent | Out-File -FilePath $envFilePath -Encoding UTF8 -NoNewline
Write-Host "✓ Created: $envFilePath" -ForegroundColor Green
Write-Host ""

# Add firewall rules
Write-Host "Configuring Windows Firewall..." -ForegroundColor Yellow
try {
    # Remove existing rules if they exist
    Remove-NetFirewallRule -DisplayName "Web Portal - Backend API" -ErrorAction SilentlyContinue
    Remove-NetFirewallRule -DisplayName "Web Portal - Frontend Dev" -ErrorAction SilentlyContinue
    
    # Add new rules
    New-NetFirewallRule -DisplayName "Web Portal - Backend API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow | Out-Null
    New-NetFirewallRule -DisplayName "Web Portal - Frontend Dev" -Direction Inbound -LocalPort 4200 -Protocol TCP -Action Allow | Out-Null
    Write-Host "✓ Firewall rules added" -ForegroundColor Green
} catch {
    Write-Host "⚠ Warning: Could not configure firewall automatically. You may need to run as Administrator." -ForegroundColor Yellow
    Write-Host "  Please manually allow ports 5000 and 4200 in Windows Firewall" -ForegroundColor Yellow
}
Write-Host ""

# Display instructions
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Update CORS in Backend:" -ForegroundColor White
Write-Host "   Edit: Backend\src\LogoDesignPortal.API\Program.cs" -ForegroundColor Gray
Write-Host "   Around line 92, add this to WithOrigins():" -ForegroundColor Gray
Write-Host "   `"http://$localIP:4200`"" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Update Backend Launch Settings (Optional):" -ForegroundColor White
Write-Host "   Edit: Backend\src\LogoDesignPortal.API\Properties\launchSettings.json" -ForegroundColor Gray
Write-Host "   Change applicationUrl to: `"http://0.0.0.0:5000`"" -ForegroundColor Cyan
Write-Host ""
Write-Host "3. Start Backend:" -ForegroundColor White
Write-Host "   cd Backend\src\LogoDesignPortal.API" -ForegroundColor Gray
Write-Host "   dotnet run --urls `"http://0.0.0.0:5000`"" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. Start Frontend:" -ForegroundColor White
Write-Host "   cd Frontend" -ForegroundColor Gray
Write-Host "   ng serve --host 0.0.0.0 --configuration network" -ForegroundColor Cyan
Write-Host "   (Note: You may need to add network configuration to angular.json)" -ForegroundColor Gray
Write-Host "   OR simply: ng serve --host 0.0.0.0" -ForegroundColor Cyan
Write-Host "   Then manually update environment.ts to use: http://$localIP:5000" -ForegroundColor Gray
Write-Host ""
Write-Host "5. Share this URL with test users:" -ForegroundColor White
Write-Host "   http://$localIP:4200" -ForegroundColor Green -BackgroundColor Black
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "NOTE: If firewall rules weren't added, run this script as Administrator." -ForegroundColor Yellow
}
