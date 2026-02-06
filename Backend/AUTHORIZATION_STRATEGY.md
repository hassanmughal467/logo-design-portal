# Authorization Strategy - Hybrid Approach

## Overview

This project uses a **hybrid authorization approach** combining:
- **Role-Based Authorization** for system-level operations
- **Permission-Based Authorization** for business operations

## Strategy

### 🔐 Role-Based Authorization (Hardcoded)

**Use for:** System-level, critical operations that should never change

**Examples:**
- Permission management (`/api/permissions/*`) - **SuperAdmin only**
- User creation (`POST /api/users`) - **SuperAdmin only**
- Authentication endpoints - Public or role-based

**Code Pattern:**
```csharp
[Authorize(Roles = "SuperAdmin")]
public async Task<IActionResult> ManagePermissions(...)
```

**Why:** These are system-level operations that should remain fixed. Only SuperAdmin should manage permissions and create users.

---

### 🎯 Permission-Based Authorization (Dynamic)

**Use for:** Business operations where SuperAdmin needs to grant/revoke access dynamically

**Examples:**
- Create Designer Profile - Can grant to Admin
- Assign Orders - Can grant to Admin
- View All Orders - Can grant to Admin
- Update Order Status - Can grant to specific roles

**Code Pattern:**
```csharp
[Authorize] // Must be authenticated
[RequirePermission("CreateDesignerProfile")] // Dynamic permission check
public async Task<IActionResult> CreateDesignerProfile(...)
```

**Why:** SuperAdmin can grant these permissions to Admin or other roles via API without code changes.

---

### 👤 Simple Role-Based (Keep as-is)

**Use for:** Simple, role-specific operations that don't need granular control

**Examples:**
- Client operations (`POST /api/orders`, `GET /api/orders/my-orders`) - **Client role only**
- Designer operations (`GET /api/orders/assigned-orders`) - **Designer role only**

**Code Pattern:**
```csharp
[Authorize(Roles = "Client")]
public async Task<IActionResult> CreateOrder(...)
```

**Why:** These are straightforward role-based operations. No need for dynamic permissions.

---

## SuperAdmin Behavior

**SuperAdmin automatically has ALL permissions:**
- Bypasses all `[RequirePermission]` checks
- Can access all endpoints (except those requiring specific roles like "Client")
- Can grant/revoke permissions to any role via `/api/permissions` endpoints

---

## How to Grant Permissions

### Step 1: Get Permission ID
```bash
GET /api/permissions
# Find the permission you want (e.g., "CreateDesignerProfile")
```

### Step 2: Assign to Role
```bash
POST /api/permissions/assign
{
  "roleId": "22222222-2222-2222-2222-222222222222", // Admin
  "permissionId": "20000000-0000-0000-0000-000000000001" // CreateDesignerProfile
}
```

### Step 3: Verify
```bash
GET /api/permissions/role/22222222-2222-2222-2222-222222222222
# Should show the new permission
```

---

## Available Permissions

### User Management
- `CreateUser` - Create new users
- `ViewUsers` - View all users
- `UpdateUser` - Update user information
- `DeleteUser` - Delete users

### Designer Profile Management
- `CreateDesignerProfile` - Create designer profiles
- `ViewDesignerProfiles` - View designer profiles
- `UpdateDesignerProfile` - Update designer profiles

### Order Management
- `CreateOrder` - Create new orders
- `ViewAllOrders` - View all orders in the system
- `AssignOrder` - Assign orders to designers
- `UpdateOrderStatus` - Update order status

### Permission Management
- `ManagePermissions` - Manage role permissions (SuperAdmin only)

### File Management
- `UploadFile` - Upload files
- `DownloadFile` - Download files
- `DeleteFile` - Delete files

---

## Decision Matrix

| Operation | Authorization Type | Reason |
|-----------|-------------------|--------|
| Manage Permissions | Role: SuperAdmin | System-level, should never change |
| Create User | Role: SuperAdmin | System-level, should never change |
| Create Designer Profile | Permission: CreateDesignerProfile | Business operation, can grant to Admin |
| Assign Order | Permission: AssignOrder | Business operation, can grant to Admin |
| View All Orders | Permission: ViewAllOrders | Business operation, can grant to Admin |
| Create Order | Role: Client | Simple role-based, no need for permissions |
| Get My Orders | Role: Client | Simple role-based, no need for permissions |
| Get Assigned Orders | Role: Designer | Simple role-based, no need for permissions |

---

## Best Practices

1. **System Operations** → Always use `[Authorize(Roles = "SuperAdmin")]`
2. **Business Operations** → Use `[RequirePermission("...")]` if SuperAdmin needs to grant access
3. **Simple Role Checks** → Use `[Authorize(Roles = "...")]` for straightforward role-based access
4. **Combine Both** → You can use both attributes together:
   ```csharp
   [Authorize(Roles = "SuperAdmin,Admin")] // Role check first
   [RequirePermission("CreateDesignerProfile")] // Then permission check
   ```

---

## Migration Path

If you want to convert an endpoint from role-based to permission-based:

1. **Before:**
   ```csharp
   [Authorize(Roles = "SuperAdmin,Admin")]
   ```

2. **After:**
   ```csharp
   [Authorize] // Remove role restriction
   [RequirePermission("YourPermissionName")] // Add permission check
   ```

3. **Grant permission to Admin:**
   ```bash
   POST /api/permissions/assign
   {
     "roleId": "22222222-2222-2222-2222-222222222222", // Admin
     "permissionId": "..." // Your permission ID
   }
   ```

---

## Summary

- ✅ **Role-Based**: System operations, simple role checks
- ✅ **Permission-Based**: Business operations needing dynamic control
- ✅ **SuperAdmin**: Has all permissions automatically
- ✅ **Flexible**: SuperAdmin can grant/revoke permissions via API
