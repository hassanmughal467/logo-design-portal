# Admin Safeguards Implementation Summary

This document summarizes the safeguards added to prevent loss of administrative access in the Admin Portal.

## 1. Root Admin Protection (Option A: `isRootAdmin` column)

### Database
- **New column**: `Users.IsRootAdmin` (bit, default: false)
- **Migration**: `20260305122103_AddIsRootAdminToUser.cs`
- **Data migration**: The first SuperAdmin user (by `CreatedAt`) is automatically set as Root Admin when the migration runs.

### Rules Enforced
- **Root Admin cannot be deleted** (soft or hard delete)
- **Root Admin cannot be deactivated** (`IsActive` cannot be set to false)
- **Root Admin role cannot be changed** (must remain SuperAdmin)
- **Root Admin can manage all other admins**

### Backend Implementation
- `UserService.SoftDeactivateUserAsync`: Returns "Root Admin cannot be deactivated." (HTTP 400)
- `UserService.HardDeleteUserAsync`: Returns "Root Admin cannot be deleted." (HTTP 400)
- `UserService.UpdateUserAsync`: Blocks role change and deactivation for Root Admin

---

## 2. Prevent Self Deletion

- **Backend**: Both `SoftDeactivateUserAsync` and `HardDeleteUserAsync` check `if (currentUserId == targetUserId)` and return "You cannot delete your own account." (HTTP 400)
- **Frontend**: Delete button is hidden for the current user; `openDeleteDialog` shows a warning toast if attempted

---

## 3. Ensure Minimum One Super Admin

Before deleting/deactivating any user with role = SuperAdmin:
- **Soft deactivate**: Counts active SuperAdmins; blocks if count ≤ 1
- **Hard delete**: Counts non-deleted SuperAdmins; blocks if count ≤ 1
- **Message**: "At least one Super Admin must remain in the system." (HTTP 400)

---

## 4. Soft Delete (Already in Place)

The system already uses soft delete:
- **IsDeleted** (BaseEntity)
- **IsActive** (User)
- **DeletedAt** (BaseEntity)

- **Soft deactivate**: Sets `IsActive = 0`, `DeactivatedAt`, `DeactivatedBy`
- **Hard delete**: Sets `IsDeleted = 1`, `IsActive = 0`, `DeletedAt`, `DeletedBy` (no record removal)

---

## 5. Angular Frontend Improvements

### Delete Button Visibility
- **Hidden for Root Admin** (`canDeleteUserFor(user)` returns false if `user.isRootAdmin`)
- **Hidden for current user** (prevents self-deletion)
- **Reactivate button** follows same rules (`canReactivateUserFor(user)`)

### Edit Dialog (Root Admin)
- Role dropdown **disabled** for Root Admin
- IsActive checkbox **disabled** for Root Admin
- Info messages: "Root Admin role cannot be changed" and "Root Admin cannot be deactivated"

### Confirmation Message
- When deleting an admin (SuperAdmin or Admin): "You are about to remove an administrator account. This action can affect system access."

---

## 6. Safety Logging (Audit Log)

Admin actions are logged via `IAuditLogService.LogActionAsync`:
- **DeactivateUser**: Logged when a user is soft-deactivated
- **DeleteUser**: Logged when a user is permanently (hard) deleted
- **RoleChange**: Logged when a user's role is changed (includes previous/new role)

Log fields: EntityType, EntityId, Action, PerformedByUserId, PerformedByRole, PreviousValue, NewValue, Timestamp, Notes

---

## Applying the Migration

```bash
cd Backend/src/LogoDesignPortal.API
dotnet ef database update --project ../LogoDesignPortal.Infrastructure/LogoDesignPortal.Infrastructure.csproj --startup-project .
```

---

## Designating Additional Root Admins (Manual)

To set another user as Root Admin (e.g., for disaster recovery), run SQL:

```sql
UPDATE Users 
SET IsRootAdmin = 1 
WHERE Id = '<user-guid>' AND RoleId = (SELECT Id FROM Roles WHERE Name = 'SuperAdmin');
```

Only SuperAdmin users should have `IsRootAdmin = 1`. Typically, only one Root Admin is recommended.
