# IIS Deployment Guide - Logo Design Portal

Deploy the Logo Design Portal on Windows IIS for testing. Uses MySQL database.

---

## Prerequisites

- **IIS** installed (Windows Features → Internet Information Services)
- **URL Rewrite Module** for IIS (for Angular routing): [Download](https://www.iis.net/downloads/microsoft/url-rewrite)
- **ASP.NET Core Hosting Bundle** for .NET 8: [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **MySQL** running with database created
- **Node.js** (for building frontend)

---

## Quick Deploy (Automated)

### 1. Update MySQL connection string

Edit `Backend\src\LogoDesignPortal.API\appsettings.Production.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=LogoDesignPortalDb;User=root;Password=YOUR_PASSWORD;"
}
```

Replace `YOUR_PASSWORD` with your MySQL password. Create the database if needed:

```sql
CREATE DATABASE LogoDesignPortalDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

> **Note:** The application uses MySQL (not SQL Server) for IIS deployment. Migrations run automatically on startup.

### 2. Run deploy script

From project root in PowerShell:

```powershell
.\deploy-to-iis.ps1
```

This will:
- Publish backend to `C:\inetpub\LogoDesignPortal\api`
- Build frontend
- Copy frontend to `C:\inetpub\LogoDesignPortal\wwwroot`

---

## Final Steps (Manual in IIS)

### Step 1: Add API site

1. Open **IIS Manager** (Win+R → `inetmgr`)
2. Right-click **Sites** → **Add Website**
3. Use:
   - **Site name**: `LogoDesignPortal-API`
   - **Physical path**: `C:\inetpub\LogoDesignPortal\api`
   - **Binding**: Type `http`, Port `5000` (or another free port)
4. Click **OK**
5. Right-click the new site → **Manage Website** → **Advanced Settings** → set **Application Pool**
6. Create/select Application Pool:
   - Right-click **Application Pools** → **Add Application Pool**
   - Name: `LogoDesignPortal-API-Pool`
   - **.NET CLR version**: **No Managed Code**
   - **Start application pool immediately**: ✓

### Step 2: Add Frontend site

1. Right-click **Sites** → **Add Website**
2. Use:
   - **Site name**: `LogoDesignPortal-Frontend`
   - **Physical path**: `C:\inetpub\LogoDesignPortal\wwwroot`
   - **Binding**: Type `http`, Port `8080` (or 80 if not in use)
3. Click **OK**

### Step 3: Update Frontend API URL (if needed)

If your API uses a different port than 5000, edit:

`Frontend\src\environments\environment.prod.ts`:

```typescript
apiUrl: 'http://localhost:YOUR_API_PORT',
```

Then run `.\deploy-to-iis.ps1` again to rebuild and redeploy.

### Step 4: Test

- **API**: http://localhost:5000/swagger
- **Frontend**: http://localhost:8080
- **Login**: superadmin@logodesign.com / SuperAdmin@123

---

## Files Folder Permissions (Required)

**If you see "Access to the path '...\Files' is denied" or "Session Expired" immediately after login**, the IIS Application Pool does not have write access to the Files folder.

### Fix: Grant permissions to the Files folder

1. **Locate your API folder** (e.g. `C:\inetpub\wwwroot\HawkBE` for site "HawkBE")
2. **Create the Files folder** if it does not exist:
   ```powershell
   New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\HawkBE\Files" -Force
   ```
3. **Grant permissions** – Right-click the `Files` folder → **Properties** → **Security** → **Edit** → **Add**:
   - Type: `IIS_IUSRS`
   - Click **Check Names** → **OK**
   - Select `IIS_IUSRS` → Check **Full control** (or at least **Modify**)
   - Click **OK**
4. **Also add the App Pool identity** (more specific):
   - Add: `IIS AppPool\HawkBE` (replace `HawkBE` with your Application Pool name)
   - Grant **Full control**
5. **Apply to subfolders**: In **Advanced** → Select the permission entry → **Edit** → Enable **Replace all child object permissions**
6. **Restart the Application Pool** in IIS Manager

### PowerShell (run as Administrator)

```powershell
$filesPath = "C:\inetpub\wwwroot\HawkBE\Files"  # Adjust path for your site
$acl = Get-Acl $filesPath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl $filesPath $acl
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| **Access to path '...\Files' is denied** / **Session Expired after login** | Grant IIS_IUSRS and IIS AppPool\YourPoolName Full Control on the Files folder. See "Files Folder Permissions" above. |
| 500.19 / 500.30 | Install ASP.NET Core Hosting Bundle, restart IIS |
| 404 on page refresh | Ensure `web.config` with URL rewrite is in wwwroot |
| CORS errors | Add your frontend URL to CORS in `Program.cs` |
| Database connection failed | Check MySQL is running, connection string in appsettings.Production.json |
| Port already in use | Use different ports (e.g. 5001 for API, 8081 for frontend) |

---

## File Locations

| Component | Path |
|-----------|------|
| API | `C:\inetpub\LogoDesignPortal\api` |
| Frontend | `C:\inetpub\LogoDesignPortal\wwwroot` |
| API Logs | `C:\inetpub\LogoDesignPortal\api\logs` |
| Uploaded Files | `C:\inetpub\LogoDesignPortal\api\Files` |
