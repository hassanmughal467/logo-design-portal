# Run this script as Administrator to configure Windows Firewall
# Right-click and select "Run with PowerShell" as Administrator

Write-Host "Configuring Windows Firewall for Web Portal..." -ForegroundColor Cyan

try {
    # Remove existing rules if they exist
    Remove-NetFirewallRule -DisplayName "Web Portal - Backend API" -ErrorAction SilentlyContinue
    Remove-NetFirewallRule -DisplayName "Web Portal - Frontend Dev" -ErrorAction SilentlyContinue
    
    # Add new rules
    New-NetFirewallRule -DisplayName "Web Portal - Backend API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
    New-NetFirewallRule -DisplayName "Web Portal - Frontend Dev" -Direction Inbound -LocalPort 4200 -Protocol TCP -Action Allow
    
    Write-Host "✓ Firewall rules configured successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ports opened:" -ForegroundColor Yellow
    Write-Host "  - Port 5000 (Backend API)" -ForegroundColor White
    Write-Host "  - Port 4200 (Frontend Dev Server)" -ForegroundColor White
} catch {
    Write-Host "✗ Error configuring firewall: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Make sure you're running as Administrator!" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Press any key to close..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
