# Fix Files folder permissions for IIS (HawkBE)
# Run this script as Administrator
# Usage: .\fix-files-permissions.ps1

$filesPath = "C:\inetpub\wwwroot\HawkBE\Files"

Write-Host "Fixing permissions for: $filesPath" -ForegroundColor Cyan

# Create folder if it doesn't exist
if (-not (Test-Path $filesPath)) {
    New-Item -ItemType Directory -Path $filesPath -Force | Out-Null
    Write-Host "Created Files folder" -ForegroundColor Green
} else {
    Write-Host "Files folder already exists" -ForegroundColor Green
}

# Create subfolders (Temporary, Permanent) if needed
$subfolders = @("Temporary", "Permanent")
foreach ($sub in $subfolders) {
    $subPath = Join-Path $filesPath $sub
    if (-not (Test-Path $subPath)) {
        New-Item -ItemType Directory -Path $subPath -Force | Out-Null
        Write-Host "Created $sub folder" -ForegroundColor Green
    }
}

# Grant permissions to IIS_IUSRS
$acl = Get-Acl $filesPath
$rule1 = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule1)
Write-Host "Added IIS_IUSRS (Full Control)" -ForegroundColor Green

# Grant permissions to App Pool identity (HawkBE)
$rule2 = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\HawkBE", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule2)
Write-Host "Added IIS AppPool\HawkBE (Full Control)" -ForegroundColor Green

Set-Acl $filesPath $acl

# Apply to subfolders
foreach ($sub in $subfolders) {
    $subPath = Join-Path $filesPath $sub
    if (Test-Path $subPath) {
        $subAcl = Get-Acl $subPath
        $subAcl.SetAccessRule($rule1)
        $subAcl.SetAccessRule($rule2)
        Set-Acl $subPath $subAcl
    }
}

Write-Host ""
Write-Host "Permissions applied successfully!" -ForegroundColor Green
Write-Host "Next step: Restart the HawkBE Application Pool in IIS Manager" -ForegroundColor Yellow
Write-Host "  (IIS Manager -> Application Pools -> HawkBE -> Restart)" -ForegroundColor Yellow
