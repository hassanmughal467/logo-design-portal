# Permission Business Flow – Standard for Every Portal

## Overview

This document describes the **standard business flow** for SuperAdmin and Admin access control. It follows industry best practices used by Salesforce, Microsoft Dynamics, HubSpot, and similar enterprise portals.

---

## Core Principle

**SuperAdmin has all rights.** SuperAdmin can create Admins and grant them **any combination of permissions** as required by the business.

---

## Business Flow

### 1. SuperAdmin Creates Admin Users

- **Who:** SuperAdmin only
- **Where:** Users → Create User
- **Action:** Create a new user and assign the **Admin** role
- **Result:** Admin user exists but has **no permissions** until granted

### 2. SuperAdmin Grants Permissions to Admin Role

- **Who:** SuperAdmin only
- **Where:** Permissions → Permission Matrix (or via API)
- **Action:** Toggle checkboxes to assign/revoke permissions for the **Admin** role
- **Result:** All users with Admin role now have the selected permissions

### 3. Granular Access Control

SuperAdmin can grant **any number** of permissions to Admin, for example:

| Permission            | Description                          | Grant to Admin? |
|-----------------------|--------------------------------------|-----------------|
| ViewAllOrders         | View all orders in the system        | ✓ or ✗          |
| AssignOrder           | Assign orders to designers           | ✓ or ✗          |
| CreateDesignerProfile | Create designer profiles             | ✓ or ✗          |
| ViewDesignerProfiles  | View designer directory              | ✓ or ✗          |
| ViewUsers             | View all users                       | ✓ or ✗          |
| ...                   | (any permission in the system)       | ✓ or ✗          |

### 4. What SuperAdmin Cannot Delegate

- **Manage Permissions** – Only SuperAdmin

### 4b. What SuperAdmin Can Delegate (optional)

- **Create User** – SuperAdmin may grant the **CreateUser** permission to **Admin**. Admins with that permission can create **Admin**, **Designer**, and **Client** users. **Only SuperAdmin** can create another **SuperAdmin**.
- **Update User** – Today, full user update (role changes) remains **SuperAdmin** only on the API; Admins typically use granular permissions elsewhere as needed.

---


## How to Use (SuperAdmin)

### Step 1: Create an Admin

1. Log in as **SuperAdmin**
2. Go to **Users** → **Create User**
3. Fill in email, name, password
4. Select role: **Admin**
5. Submit

### Step 2: Grant Permissions to Admin Role

1. Go to **Permissions** (Administration section)
2. Click **Permission Matrix**
3. Find the **Admin** column
4. Check the permissions you want Admins to have
5. Uncheck permissions you want to revoke

Changes apply immediately to all Admin users.

### Step 3: Verify

- Log in as the new Admin
- Confirm they can access only the features you granted
- Menus and actions are shown/hidden based on permissions

---

## API Reference (for integrations)

### Get all permissions
```
GET /api/permissions
```

### Get permissions for a role
```
GET /api/permissions/role/{roleId}
```

### Assign permission to role
```
POST /api/permissions/assign
{
  "roleId": "<Admin role GUID>",
  "permissionId": "<Permission GUID>"
}
```

### Revoke permission from role
```
DELETE /api/permissions/revoke
{
  "roleId": "<Admin role GUID>",
  "permissionId": "<Permission GUID>"
}
```

### Role IDs (standard seed)
- SuperAdmin: `11111111-1111-1111-1111-111111111111`
- Admin: `22222222-2222-2222-2222-222222222222`
- Designer: `33333333-3333-3333-3333-333333333333`
- Client: `44444444-4444-4444-4444-444444444444`

---

## Summary

| Actor      | Can Do                                                                 |
|-----------|-------------------------------------------------------------------------|
| SuperAdmin | All rights, create Admins, grant/revoke any permissions to any role    |
| Admin     | Only what SuperAdmin has granted via Permission Matrix                 |
| Designer  | Fixed Designer access (no permission matrix)                           |
| Client    | Fixed Client access (own data only)                                    |

This flow is the **standard for every portal**: one root admin with full control, and configurable access for other admin roles.
