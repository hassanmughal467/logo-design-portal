# 🧪 Complete Testing Guide - Logo Design Portal Backend

## 📋 Table of Contents
1. [Prerequisites & Setup](#prerequisites--setup)
2. [Database Setup](#database-setup)
3. [Running the Application](#running-the-application)
4. [Testing via Swagger](#testing-via-swagger)
5. [Complete Test Scenarios](#complete-test-scenarios)
6. [Testing Different Roles](#testing-different-roles)
7. [File Upload Testing](#file-upload-testing)
8. [Troubleshooting](#troubleshooting)

---

## 🚀 Prerequisites & Setup

### Required Software

You need the following software installed on your machine:

1. **.NET 8 SDK** (Software Development Kit)
2. **SQL Server** (LocalDB or Full SQL Server)
3. **Git** (for version control)
4. **Code Editor** (Visual Studio, VS Code, or Rider - optional but recommended)

---

### 1. Install .NET 8 SDK

#### What is .NET SDK?
The .NET SDK is Microsoft's development platform that includes everything you need to build and run .NET applications.

#### Installation Steps:

**Option A: Download from Microsoft (Recommended)**
1. Go to: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download **.NET 8 SDK** (not just Runtime)
3. Run the installer
4. Follow the installation wizard
5. Restart your computer if prompted

**Option B: Using Chocolatey (Windows)**
```powershell
# If you have Chocolatey installed
choco install dotnet-8.0-sdk
```

**Option C: Using Winget (Windows 10/11)**
```powershell
winget install Microsoft.DotNet.SDK.8
```

#### Verify Installation:
```bash
# Open Command Prompt or PowerShell
dotnet --version
# Should output: 8.0.xxx (e.g., 8.0.100)

# Check if SDK is installed (not just runtime)
dotnet --list-sdks
# Should show: 8.0.xxx [C:\Program Files\dotnet\sdk]
```

**Troubleshooting:**
- If `dotnet` command not found: Add to PATH or restart terminal
- If version is lower than 8.0: Update or install .NET 8 SDK
- If only runtime installed: Install SDK (includes runtime)

---

### 2. Install SQL Server

#### What is SQL Server?
SQL Server is a database management system that stores all your application data.

#### Option A: SQL Server LocalDB (Recommended for Development)

**What is LocalDB?**
- Lightweight version of SQL Server
- Perfect for development and testing
- Automatically starts when needed
- Free and included with Visual Studio

**Installation Steps:**

1. **If you have Visual Studio installed:**
   - LocalDB is usually already installed
   - Check if it's available

2. **If you don't have Visual Studio:**
   - Download SQL Server Express: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - During installation, select "LocalDB" option
   - Or download SQL Server Express LocalDB directly

3. **Verify Installation:**
   ```powershell
   # Check LocalDB
   sqllocaldb info
   
   # Start LocalDB (if not running)
   sqllocaldb start MSSQLLocalDB
   
   # Check LocalDB instances
   sqllocaldb info MSSQLLocalDB
   ```

**Expected Output:**
```
Name:               MSSQLLocalDB
Version:            15.0.2000.5
Shared name:        
Owner:              YourComputer\YourUsername
Auto-create:        Yes
State:              Running
Last start time:    1/1/2024 10:00:00 AM
Instance pipe name: np:\\.\pipe\LOCALDB#...
```

#### Option B: Full SQL Server (For Production-like Testing)

**Installation:**
1. Download SQL Server Express (Free): https://www.microsoft.com/en-us/sql-server/sql-server-downloads
2. Run installer
3. Choose "Basic" installation type
4. Set authentication mode (Mixed Mode recommended)
5. Set a password for 'sa' account
6. Complete installation

**Connection String (if using Full SQL Server):**
```json
"Server=localhost;Database=LogoDesignPortalDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"
```

#### Option C: SQL Server in Docker (Advanced)

```bash
# Pull SQL Server image
docker pull mcr.microsoft.com/mssql/server:2022-latest

# Run SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password123" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

---

### 3. Install Git (If Not Already Installed)

#### What is Git?
Git is a version control system used to track changes in your code.

#### Installation:

**Windows:**
1. Download from: https://git-scm.com/download/win
2. Run installer
3. Use default settings (recommended)
4. Verify installation:
   ```bash
   git --version
   # Should show: git version 2.xx.x
   ```

**Verify Git is Working:**
```bash
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

---

### 4. Code Editor (Optional but Recommended)

#### Option A: Visual Studio 2022 (Recommended for .NET)
- **Download:** https://visualstudio.microsoft.com/downloads/
- **Install:** Select "ASP.NET and web development" workload
- **Includes:** .NET SDK, SQL Server LocalDB, Git

#### Option B: Visual Studio Code (Lightweight)
- **Download:** https://code.visualstudio.com/
- **Extensions to Install:**
  - C# (by Microsoft)
  - .NET Extension Pack
  - SQL Server (mssql)

#### Option C: JetBrains Rider (Professional)
- **Download:** https://www.jetbrains.com/rider/
- **Note:** Paid (free trial available)

---

### 5. System Requirements

#### Minimum Requirements:
- **OS:** Windows 10/11, macOS 10.15+, or Linux
- **RAM:** 4 GB (8 GB recommended)
- **Disk Space:** 2 GB free space
- **Processor:** 1.6 GHz or faster

#### Recommended Requirements:
- **OS:** Windows 11 or latest macOS/Linux
- **RAM:** 8 GB or more
- **Disk Space:** 10 GB free space (for SQL Server and tools)
- **Processor:** Multi-core processor

---

### 6. Verify All Prerequisites

Run these commands to verify everything is installed:

```powershell
# Check .NET SDK
dotnet --version
# Expected: 8.0.xxx

# Check .NET SDK list
dotnet --list-sdks
# Expected: Shows 8.0.xxx

# Check SQL Server LocalDB
sqllocaldb info
# Expected: Shows LocalDB information

# Check Git
git --version
# Expected: git version 2.xx.x

# Check if you can build .NET projects
dotnet --info
# Expected: Shows .NET information
```

**Expected Output Example:**
```
.NET SDK:
 Version:  8.0.100
 Location: C:\Program Files\dotnet\sdk

SQL Server LocalDB:
 Name: MSSQLLocalDB
 State: Running

Git:
 git version 2.42.0.windows.2
```

---

### 7. Navigate to Project

```bash
# Open Command Prompt or PowerShell
cd "C:\Users\MuhammadHassan\Desktop\Web Portal\Backend"

# Or if you're in the Web Portal folder
cd Backend
```

---

### 8. Restore NuGet Packages

NuGet packages are external libraries your project depends on.

```bash
# Restore all packages
dotnet restore

# Or restore for specific project
cd src/LogoDesignPortal.API
dotnet restore
```

**What happens:**
- Downloads all required packages (EF Core, JWT, AutoMapper, etc.)
- Creates `obj/` and `bin/` folders
- May take a few minutes on first run

**Expected Output:**
```
  Determining projects to restore...
  Restored C:\...\LogoDesignPortal.Domain.csproj
  Restored C:\...\LogoDesignPortal.Application.csproj
  Restored C:\...\LogoDesignPortal.Infrastructure.csproj
  Restored C:\...\LogoDesignPortal.API.csproj
```

**Troubleshooting:**
- If restore fails: Check internet connection
- If package errors: Clear NuGet cache: `dotnet nuget locals all --clear`
- If still fails: Delete `bin/` and `obj/` folders, then restore again

---

### 9. Common Installation Issues & Solutions

#### Issue: "dotnet command not found"
**Solution:**
1. Restart your terminal/command prompt
2. Check PATH environment variable includes: `C:\Program Files\dotnet`
3. Reinstall .NET SDK if needed

#### Issue: "SQL Server connection failed"
**Solution:**
1. Start LocalDB: `sqllocaldb start MSSQLLocalDB`
2. Check connection string in `appsettings.json`
3. Verify SQL Server service is running

#### Issue: "Package restore failed"
**Solution:**
1. Check internet connection
2. Clear NuGet cache: `dotnet nuget locals all --clear`
3. Delete `bin/` and `obj/` folders
4. Try restore again

#### Issue: "Port 5000/5001 already in use"
**Solution:**
1. Find process using port: `netstat -ano | findstr :5000`
2. Kill process or change port in `launchSettings.json`

---

### 10. Quick Setup Checklist

Before running the application, verify:

- [ ] .NET 8 SDK installed (`dotnet --version` shows 8.0.x)
- [ ] SQL Server LocalDB available (`sqllocaldb info` works)
- [ ] Git installed (`git --version` works)
- [ ] Project files downloaded/cloned
- [ ] Packages restored (`dotnet restore` completed)
- [ ] Connection string updated in `appsettings.json`
- [ ] JWT key configured in `appsettings.json`

---

### 11. First-Time Setup Summary

**Complete setup process:**

1. **Install .NET 8 SDK** (5-10 minutes)
   ```bash
   # Verify
   dotnet --version
   ```

2. **Install SQL Server LocalDB** (5-10 minutes)
   ```bash
   # Verify
   sqllocaldb start MSSQLLocalDB
   sqllocaldb info
   ```

3. **Install Git** (if needed) (2-5 minutes)
   ```bash
   # Verify
   git --version
   ```

4. **Clone/Download Project** (1-2 minutes)
   ```bash
   git clone <repository-url>
   # or extract downloaded zip
   ```

5. **Navigate to Backend** (10 seconds)
   ```bash
   cd Backend
   ```

6. **Restore Packages** (2-5 minutes)
   ```bash
   dotnet restore
   ```

7. **Update Configuration** (1 minute)
   - Edit `appsettings.json` if needed

8. **Run Application** (30 seconds)
   ```bash
   cd src/LogoDesignPortal.API
   dotnet run
   ```

**Total Setup Time:** ~15-30 minutes (depending on internet speed)

---

### 12. Additional Tools (Optional)

#### Postman (API Testing)
- **Download:** https://www.postman.com/downloads/
- **Use for:** Testing API endpoints outside Swagger

#### SQL Server Management Studio (SSMS)
- **Download:** https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
- **Use for:** Viewing and managing database directly

#### Azure Data Studio (Alternative to SSMS)
- **Download:** https://learn.microsoft.com/en-us/sql/azure-data-studio/download-azure-data-studio
- **Use for:** Modern database management tool

---

## 🗄️ Database Setup

### Option 1: Automatic (Recommended for Testing)
The application will automatically create the database on first run using `EnsureCreated()`.

### Option 2: Using Migrations (Production-like)
```bash
# Navigate to Infrastructure project
cd src/LogoDesignPortal.Infrastructure

# Create migration
dotnet ef migrations add InitialCreate --startup-project ../LogoDesignPortal.API

# Apply migration
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

### Verify Database Connection
Update `src/LogoDesignPortal.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LogoDesignPortalDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

---

## ▶️ Running the Application

### Start the API
```bash
cd src/LogoDesignPortal.API
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### Access Swagger
Open browser: **https://localhost:5001/swagger**

---

## 🔍 Testing via Swagger

### Step 1: Open Swagger UI
1. Navigate to `https://localhost:5001/swagger`
2. You should see all API endpoints organized by controllers

### Step 2: Authorize in Swagger
1. Click the **"Authorize"** button (top right, lock icon)
2. Enter: `Bearer <your-token-here>`
3. Click **"Authorize"**
4. Click **"Close"**

---

## 📝 Complete Test Scenarios

## Test Scenario 1: Client Registration & Login

### 1.1 Register a New Client
**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "email": "client1@test.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe",
  "companyName": "Acme Corporation",
  "phoneNumber": "+1234567890"
}
```

**Expected Response (201 Created):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "guid-here",
    "email": "client1@test.com",
    "firstName": "John",
    "lastName": "Doe",
    "roleName": "Client"
  }
}
```

**✅ Save the `token` and `refreshToken` for later use!**

### 1.2 Login with Registered Client
**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "email": "client1@test.com",
  "password": "Password123!"
}
```

**Expected Response:** Same as registration (new tokens)

### 1.3 Test Invalid Login
**Request Body:**
```json
{
  "email": "client1@test.com",
  "password": "WrongPassword"
}
```

**Expected Response (401 Unauthorized):**
```json
{
  "error": "Invalid email or password."
}
```

---

## Test Scenario 2: Refresh Token

### 2.1 Refresh Access Token
**Endpoint:** `POST /api/auth/refresh-token`

**Request Body:**
```json
{
  "token": "your-access-token-here",
  "refreshToken": "your-refresh-token-here"
}
```

**Expected Response (200 OK):**
```json
{
  "token": "new-access-token",
  "refreshToken": "new-refresh-token",
  "expiresAt": "2024-01-01T13:00:00Z",
  "user": { ... }
}
```

---

## Test Scenario 3: Create Logo Order (Client)

### 3.1 Authorize as Client
1. Use the token from registration/login
2. Click **"Authorize"** in Swagger
3. Enter: `Bearer <client-token>`

### 3.2 Create an Order
**Endpoint:** `POST /api/orders`

**Request Body:**
```json
{
  "title": "Modern Tech Logo",
  "description": "Need a modern, minimalist logo for our tech startup. Should represent innovation and technology.",
  "price": 500.00,
  "deadline": "2024-12-31T00:00:00Z",
  "requirements": "Logo should work in both color and black/white. Need vector format.",
  "colorPreferences": "Blue (#0066CC) and White",
  "stylePreferences": "Modern, minimalist, professional"
}
```

**Expected Response (201 Created):**
```json
{
  "id": "order-guid",
  "title": "Modern Tech Logo",
  "description": "...",
  "status": "Pending",
  "price": 500.00,
  "deadline": "2024-12-31T00:00:00Z",
  "createdAt": "2024-01-01T10:00:00Z",
  "client": {
    "id": "client-guid",
    "companyName": "Acme Corporation",
    "email": "client1@test.com",
    "phoneNumber": "+1234567890"
  },
  "designer": null,
  "fileCount": 0
}
```

**✅ Save the order `id` for next tests!**

### 3.3 Get My Orders
**Endpoint:** `GET /api/orders/my-orders`

**Expected Response (200 OK):**
```json
[
  {
    "id": "order-guid",
    "title": "Modern Tech Logo",
    "status": "Pending",
    ...
  }
]
```

### 3.4 Get Order by ID
**Endpoint:** `GET /api/orders/{id}`

Replace `{id}` with your order ID.

**Expected Response:** Order details

---

## Test Scenario 4: SuperAdmin Operations

### 4.1 Login as SuperAdmin
**Default Credentials:**
- Email: `superadmin@logodesign.com`
- Password: `SuperAdmin@123`

**Endpoint:** `POST /api/auth/login`

**✅ Save SuperAdmin token!**

### 4.2 Create a Designer User
**Endpoint:** `POST /api/users`

**Request Body:**
```json
{
  "email": "designer1@test.com",
  "password": "Designer123!",
  "firstName": "Jane",
  "lastName": "Designer",
  "roleId": "33333333-3333-3333-3333-333333333333"
}
```

**Note:** Role IDs:
- SuperAdmin: `11111111-1111-1111-1111-111111111111`
- Admin: `22222222-2222-2222-2222-222222222222`
- Designer: `33333333-3333-3333-3333-333333333333`
- Client: `44444444-4444-4444-4444-444444444444`

**✅ Save the designer user ID from the response!**

### 4.3 Create Designer Profile (NEW - Required Step)
**Endpoint:** `POST /api/users/designer-profiles`

**Important:** After creating a Designer user, you MUST create their DesignerProfile before they can be assigned orders.

**Request Body:**
```json
{
  "userId": "designer-user-id-from-step-4.2",
  "specialization": "Logo Design, Brand Identity",
  "bio": "Experienced logo designer with 5+ years in brand identity",
  "hourlyRate": 50.00,
  "isAvailable": true
}
```

**Expected Response (201 Created):**
```json
{
  "id": "profile-guid",
  "userId": "designer-user-id",
  "userEmail": "designer1@test.com",
  "userFirstName": "Jane",
  "userLastName": "Designer",
  "specialization": "Logo Design, Brand Identity",
  "bio": "Experienced logo designer...",
  "hourlyRate": 50.00,
  "isAvailable": true,
  "createdAt": "2024-01-01T10:00:00Z"
}
```

**✅ Designer profile created! Now the designer can be assigned orders.**

### 4.4 Get All Orders (SuperAdmin)
**Endpoint:** `GET /api/orders`

**Expected Response:** List of all orders with full client info

### 4.5 Assign Order to Designer
**Endpoint:** `POST /api/orders/{orderId}/assign`

**Important:** The designer MUST have a DesignerProfile created (from step 4.3) before assignment.

**Request Body:**
```json
{
  "designerId": "designer-user-id" // Use the User ID, not Role ID
}
```

**Expected Response:** Order with status changed to "InProgress" and designer assigned

---

## Test Scenario 4.5: Permission Management (NEW)

### 4.5.1 Get All Permissions
**Endpoint:** `GET /api/permissions`

**Expected Response:** List of all available permissions

**Example Response:**
```json
[
  {
    "id": "20000000-0000-0000-0000-000000000001",
    "name": "CreateDesignerProfile",
    "description": "Create designer profiles",
    "resource": "DesignerProfile",
    "action": "Create"
  },
  {
    "id": "30000000-0000-0000-0000-000000000003",
    "name": "AssignOrder",
    "description": "Assign orders to designers",
    "resource": "Order",
    "action": "Assign"
  }
  // ... more permissions
]
```

### 4.5.2 Get Role Permissions
**Endpoint:** `GET /api/permissions/role/{roleId}`

**Example:** `GET /api/permissions/role/22222222-2222-2222-2222-222222222222` (Admin)

**Expected Response:**
```json
{
  "roleId": "22222222-2222-2222-2222-222222222222",
  "roleName": "Admin",
  "permissions": [
    {
      "id": "...",
      "name": "ViewDesignerProfiles",
      "description": "View designer profiles",
      "resource": "DesignerProfile",
      "action": "Read"
    }
  ]
}
```

### 4.5.3 Grant Permission to Role (SuperAdmin Only)
**Endpoint:** `POST /api/permissions/assign`

**Example: Grant Admin the ability to create designer profiles**

**Request Body:**
```json
{
  "roleId": "22222222-2222-2222-2222-222222222222", // Admin
  "permissionId": "20000000-0000-0000-0000-000000000001" // CreateDesignerProfile
}
```

**Expected Response (200 OK):**
```json
{
  "message": "Permission assigned successfully."
}
```

**Now Admin can create designer profiles!**

### 4.5.4 Revoke Permission from Role
**Endpoint:** `DELETE /api/permissions/revoke`

**Request Body:**
```json
{
  "roleId": "22222222-2222-2222-2222-222222222222", // Admin
  "permissionId": "20000000-0000-0000-0000-000000000001" // CreateDesignerProfile
}
```

**Expected Response (200 OK):**
```json
{
  "message": "Permission revoked successfully."
}
```

**Note:** SuperAdmin always has all permissions automatically (cannot be revoked).

---

## Test Scenario 5: Designer Operations

### 5.1 Login as Designer
Use the designer credentials created by SuperAdmin.

### 5.2 Get Assigned Orders
**Endpoint:** `GET /api/orders/assigned-orders`

**Expected Response:** 
- ✅ Should show assigned orders
- ✅ **Client info should be NULL** (privacy protection)

### 5.3 Get Order Details (Designer)
**Endpoint:** `GET /api/orders/{id}`

**Expected Response:**
- ✅ Order details visible
- ✅ **Client object should be NULL** (no client identity)

### 5.4 Update Order Status (Designer)
**Endpoint:** `PUT /api/orders/{id}/status`

**Request Body:**
```json
{
  "status": "Completed",
  "notes": "Logo design completed and approved"
}
```

**Expected Response:** Updated order

---

## Test Scenario 7: File Upload & Download

### 7.1 Upload File (Client)
**Endpoint:** `POST /api/files/upload/{orderId}`

**In Swagger:**
1. Select the endpoint
2. Click "Try it out"
3. Enter order ID
4. Click "Choose File" and select an image (JPG, PNG, SVG, etc.)
5. Click "Execute"

**Expected Response (200 OK):**
```json
{
  "id": "file-guid",
  "fileName": "generated-filename.jpg",
  "originalFileName": "my-logo.jpg",
  "fileSize": 123456,
  "contentType": "image/jpeg",
  "uploadedAt": "2024-01-01T10:00:00Z"
}
```

**File Requirements:**
- Max size: 10MB
- Allowed types: .jpg, .jpeg, .png, .gif, .svg, .pdf, .ai, .eps, .psd

### 7.2 Get Order Files
**Endpoint:** `GET /api/files/order/{orderId}`

**Expected Response:** List of files for the order

### 7.3 Download File
**Endpoint:** `GET /api/files/{fileId}/download`

**Expected Response:** File download (binary)

**Note:** In Swagger, this will show as base64. Use Postman or browser for actual download.

### 7.4 Test File Authorization
Try downloading a file from another client's order - should get **401 Unauthorized**

---

## Test Scenario 8: Admin Role Testing

### 8.1 Create Admin User (SuperAdmin)
**Endpoint:** `POST /api/users`

**Request Body:**
```json
{
  "email": "admin1@test.com",
  "password": "Admin123!",
  "firstName": "Admin",
  "lastName": "User",
  "roleId": "22222222-2222-2222-2222-222222222222"
}
```

### 8.2 Login as Admin
Use admin credentials.

### 8.3 Get All Orders (Admin)
**Endpoint:** `GET /api/orders`

**Expected Response:**
- ✅ Should see all orders
- ✅ **Client email/phone should be NULL** (masked)
- ✅ Client company name visible

### 8.4 Verify Client Info Masking
Check order response - client object should have:
```json
{
  "client": {
    "id": "guid",
    "companyName": "Acme Corporation",
    "email": null,  // ✅ Masked
    "phoneNumber": null  // ✅ Masked
  }
}
```

---

## Test Scenario 8: Authorization Testing

### 8.1 Test Unauthorized Access
1. **Without token:** Try any protected endpoint
   - Expected: **401 Unauthorized**

2. **Wrong role:** Client trying to access `/api/orders` (all orders)
   - Expected: **403 Forbidden**

3. **Other client's order:** Client trying to access another client's order
   - Expected: **401 Unauthorized**

### 8.2 Test Token Expiry
1. Wait 1 hour (or modify token expiry in code)
2. Try accessing protected endpoint
3. Expected: **401 Unauthorized**
4. Use refresh token to get new access token

---

## Test Scenario 9: Order Status Flow

### Complete Order Lifecycle:
1. **Client creates order** → Status: `Pending`
2. **SuperAdmin assigns to Designer** → Status: `InProgress`
3. **Designer uploads files** → Files visible
4. **Designer updates status** → Status: `Completed`
5. **Client downloads files** → Files accessible

### Test Status History
Check database or add endpoint to see status history:
```sql
SELECT * FROM OrderStatusHistories WHERE OrderId = 'your-order-id'
```

---

## 🧪 Testing Different Roles

### Quick Role Test Matrix

| Action | SuperAdmin | Admin* | Designer | Client |
|--------|-----------|--------|----------|--------|
| Register | ❌ | ❌ | ❌ | ✅ |
| Create Order | ❌ | ❌ | ❌ | ✅ |
| View All Orders | ✅ | ✅* | ❌ | ❌ |
| View Own Orders | ✅ | ✅ | ✅ | ✅ |
| Assign Order | ✅ | ✅* | ❌ | ❌ |
| Create Designer Profile | ✅ | ✅* | ❌ | ❌ |
| Manage Permissions | ✅ | ❌ | ❌ | ❌ |
| Upload File | ✅ | ✅ | ✅ (assigned) | ✅ (own) |
| See Client Email | ✅ | ❌ | ❌ | ✅ (own) |
| See Client Identity | ✅ | ❌ | ❌ | ✅ (own) |

**Note:** *Admin permissions depend on what SuperAdmin grants via `/api/permissions/assign`. By default, Admin has no permissions unless SuperAdmin grants them.

---

## 📁 File Upload Testing

### Using Swagger
1. Select `POST /api/files/upload/{orderId}`
2. Click "Try it out"
3. Enter order ID
4. Click "Choose File"
5. Select image file (< 10MB)
6. Click "Execute"

### Using Postman
1. Method: `POST`
2. URL: `https://localhost:5001/api/files/upload/{orderId}`
3. Headers:
   - `Authorization: Bearer <token>`
4. Body: `form-data`
   - Key: `file` (type: File)
   - Value: Select file
5. Send

### Using cURL
```bash
curl -X POST "https://localhost:5001/api/files/upload/{orderId}" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@/path/to/image.jpg"
```

### Test File Restrictions
1. **Large file (>10MB):** Should fail with error
2. **Invalid type (.txt):** Should fail with error
3. **No file:** Should fail with error

---

## 🔧 Troubleshooting

### Issue: Database Connection Error
**Solution:**
```bash
# Check SQL Server LocalDB
sqllocaldb start MSSQLLocalDB
sqllocaldb info MSSQLLocalDB

# Update connection string if needed
```

### Issue: 401 Unauthorized
**Solutions:**
1. Check token is valid (not expired)
2. Ensure token format: `Bearer <token>` (with space)
3. Verify user is active in database
4. Check role matches endpoint requirement

### Issue: 500 Internal Server Error
**Solutions:**
1. Check application logs
2. Verify database exists
3. Check file storage path exists
4. Verify all required fields in request

### Issue: File Upload Fails
**Solutions:**
1. Check file size (< 10MB)
2. Verify file type is allowed
3. Ensure `Files` directory exists (auto-created)
4. Check file permissions

### Issue: Swagger Not Loading
**Solutions:**
1. Ensure running in Development mode
2. Check HTTPS certificate
3. Try HTTP instead: `http://localhost:5000/swagger`

### Issue: Refresh Token Not Working
**Solutions:**
1. Verify both tokens are from same user
2. Check refresh token hasn't expired (7 days)
3. Ensure tokens are sent correctly

---

## 📊 Database Verification

### Check Seeded Data
```sql
-- Check roles
SELECT * FROM Roles;

-- Check SuperAdmin user
SELECT * FROM Users WHERE Email = 'superadmin@logodesign.com';

-- Check orders
SELECT * FROM LogoOrders;

-- Check files
SELECT * FROM LogoFiles;
```

### Verify Soft Delete
```sql
-- Check deleted items (should not appear in normal queries)
SELECT * FROM Users WHERE IsDeleted = 1;
```

---

## ✅ Complete Test Checklist

- [ ] Client registration works
- [ ] Client login works
- [ ] Refresh token works
- [ ] Client can create order
- [ ] Client can view own orders
- [ ] SuperAdmin can create users
- [ ] SuperAdmin can create designer profiles
- [ ] SuperAdmin can view all orders
- [ ] SuperAdmin can assign orders
- [ ] SuperAdmin can grant permissions to Admin
- [ ] SuperAdmin can revoke permissions from Admin
- [ ] Admin can access endpoints based on granted permissions
- [ ] Designer can view assigned orders
- [ ] Designer cannot see client identity
- [ ] Permission system works correctly (grant/revoke)
- [ ] Admin can view orders but client email masked
- [ ] File upload works
- [ ] File download works
- [ ] File authorization works
- [ ] Order status updates work
- [ ] Authorization rules enforced
- [ ] Soft delete works
- [ ] Audit fields populated

---

## 🎯 Quick Test Commands

### Test Registration
```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@test.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User",
    "companyName": "Test Corp"
  }'
```

### Test Login
```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@test.com",
    "password": "Test123!"
  }'
```

---

## 📝 Notes

- **Default SuperAdmin:** `superadmin@logodesign.com` / `SuperAdmin@123`
- **Token Expiry:** Access token = 1 hour, Refresh token = 7 days
- **File Storage:** Files stored in `Files/` directory (relative to API)
- **Database:** Auto-created on first run
- **Swagger:** Available at `/swagger` in Development mode

---

## 🚀 Next Steps After Testing

1. Create more test users (different roles)
2. Test edge cases (empty strings, null values)
3. Test concurrent requests
4. Test file size limits
5. Test authorization boundaries
6. Verify audit fields in database
7. Test soft delete functionality

Happy Testing! 🎉
