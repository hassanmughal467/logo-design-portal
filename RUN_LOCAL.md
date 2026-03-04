# Run Web Portal on Local Machine

## Quick Start (2 steps)

### 1. Start Backend
Double-click **`start-backend.bat`** or run:
```powershell
cd Backend\src\LogoDesignPortal.API
dotnet run --launch-profile https
```
Wait for: `Now listening on: https://localhost:5001`

### 2. Start Frontend
Double-click **`start-frontend.bat`** or run:
```powershell
cd Frontend
ng serve --host 0.0.0.0
```
Wait for: `Application bundle generation complete`

---

## Access the App

| What | URL |
|------|-----|
| **Login** | http://localhost:4200/auth/login |
| **Swagger API** | https://localhost:5001/swagger |

### Default Login
- **Email:** superadmin@logodesign.com  
- **Password:** SuperAdmin@123

---

## Database (MySQL)

The app uses **MySQL** for local development. Ensure MySQL is running on your machine.

**First-time setup:**
1. Create the database in MySQL:
   ```sql
   CREATE DATABASE LogoDesignPortalDb;
   ```
2. Update the connection string in `appsettings.json` or `appsettings.Development.json` if your MySQL uses different credentials (User, Password, Port).

Migrations run automatically on startup. SuperAdmin user is seeded automatically.

---

## Important: Stop Running Processes First

If you have the backend running in **Visual Studio** or another terminal, **stop it first** before building or running from a new window. Having multiple instances can cause file lock errors.

## Troubleshooting

**Backend won't start?**
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Run migrations manually: `dotnet ef database update --project Backend\src\LogoDesignPortal.Infrastructure --startup-project Backend\src\LogoDesignPortal.API`

**Frontend won't start?**
- Ensure Node.js is installed: `node --version`
- Run `npm install` in the Frontend folder

**Login fails with 500?**
- Check the backend console for the actual error
- Ensure backend is fully started before logging in
- Try https://localhost:5001/swagger to verify API is running
