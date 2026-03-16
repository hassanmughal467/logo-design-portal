# IIS Deployment Failure Analysis – HTTP 500.30

## Root Cause Summary

**HTTP Error 500.30** means the ASP.NET Core app crashed **during startup**, before it could handle any requests. The CORS error in the browser is a **side effect**—the API never starts, so the browser receives no valid response and reports a CORS error.

The most likely cause is **database connection failure** during the startup migration/seed block in `Program.cs` (lines 192–250).

---

## 1. Database Connection String (Primary Cause)

### Problem

`appsettings.Production.json` contains a placeholder password:

```json
"DefaultConnection": "Server=localhost;Database=LogoDesignPortalDb;User=root;Password=YOUR_MYSQL_PASSWORD;"
```

If `YOUR_MYSQL_PASSWORD` is not replaced with the real MySQL password, the app will fail when:

1. EF Core tries to connect during `AddInfrastructure`
2. The startup block runs `context.Database.GetPendingMigrationsAsync()` and `MigrateAsync()`

Typical MySQL errors:

- `Access denied for user 'root'@'localhost'`
- `Unable to connect to any of the specified MySQL hosts`

### Fix

Use environment variables in `web.config` so you never store real credentials in config files:

```xml
<environmentVariables>
  <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  <environmentVariable name="ConnectionStrings__DefaultConnection" value="Server=localhost;Port=3306;Database=LogoDesignPortalDb;User=root;Password=YOUR_ACTUAL_PASSWORD;" />
</environmentVariables>
```

Replace `YOUR_ACTUAL_PASSWORD` with the real MySQL password. If MySQL is on another host, use that host instead of `localhost`.

---

## 2. JWT Configuration

### Problem

`Program.cs` (lines 65–68) throws if JWT settings are missing:

```csharp
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured...");
```

If environment variables override these with empty values, startup will fail.

### Status

`appsettings.Production.json` has valid JWT values. Ensure no IIS or system env vars set `Jwt__Key`, `Jwt__Issuer`, or `Jwt__Audience` to empty strings.

---

## 3. Database Migration at Startup

### Problem

`Program.cs` runs migrations and seeding **before** `app.Run()`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.GetPendingMigrationsAsync();
    await context.Database.MigrateAsync();
    // ... seeding ...
}
```

Any failure here (connection, migration, seed) causes the app to crash.

### Fix

1. Fix the connection string (see above).
2. Check IIS stdout logs for the exact exception:
   - Path: `{deployed-app-folder}\logs\stdout_*.log`
   - Example: `C:\inetpub\LogoDesignPortal\api\logs\stdout_20250317120000_1234.log`

---

## 4. CORS Configuration

### Status

CORS is correctly configured in `Program.cs`:

```csharp
.WithOrigins(
    "http://admin.hawkmerchandising.com",
    "https://admin.hawkmerchandising.com",
    ...
)
```

`admin.hawkmerchandising.com` is allowed. The CORS error in the browser is because the API never starts; once the API runs, CORS will work.

---

## 5. Middleware Order

### Status

Order is correct:

1. `UseForwardedHeaders()` – for IIS reverse proxy
2. `UseCors("AllowAdmin")` – before auth so OPTIONS preflight succeeds
3. `UseMiddleware<ExceptionMiddleware>()`
4. `UseHttpsRedirection()` (non-Development only)
5. `UseAuthentication()` / `UseAuthorization()`
6. `UseMiddleware<RateLimitingMiddleware>()`

---

## 6. File Storage Path

### Status

`FileStorage:Path` is `"Files"` (relative). On IIS, `Directory.GetCurrentDirectory()` is the app directory, so this should work. If the app pool identity lacks write permission, `FileStorageInitializer` logs a warning but does not crash.

---

## 7. Service Registrations

### Status

- DbContext, JWT, CORS, SignalR, hosted services are registered correctly.
- `ProductionSafetyOptions` is configured; missing section falls back to defaults.

---

## 8. Environment-Specific Configuration

### Gaps in `appsettings.Production.json`

1. **Connection string** – placeholder password (see above).
2. **Email:FrontendUrl** – set to `"http://localhost"`; should be `"https://admin.hawkmerchandising.com"` for production.
3. **Cors:AllowedOrigins** – does not include `admin.hawkmerchandising.com`, but `Program.cs` uses a hardcoded policy, so this is not used. No change needed for CORS.

---

## Immediate Actions

1. **Check stdout logs**  
   Open the latest `logs\stdout_*.log` in the deployed API folder and look for the exception message.

2. **Fix connection string**  
   - Either update `appsettings.Production.json` with the real password, or  
   - Add `ConnectionStrings__DefaultConnection` in `web.config` (recommended).

3. **Verify MySQL**  
   - MySQL is running on the server.  
   - `root` can connect with the configured password.  
   - Database `LogoDesignPortalDb` exists (or can be created).

4. **Recycle app pool**  
   IIS Manager → Application Pools → right-click your pool → Recycle.

---

## Quick Checklist

| Item | Action |
|------|--------|
| Connection string | Replace `YOUR_MYSQL_PASSWORD` or use env var in web.config |
| MySQL | Running and reachable from the API server |
| stdout logs | Inspect `logs\stdout_*.log` for the real error |
| Email:FrontendUrl | Set to `https://admin.hawkmerchandising.com` in Production |
| App pool | Recycle after config changes |
