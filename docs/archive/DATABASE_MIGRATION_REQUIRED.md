# Database Migration Required

## ⚠️ Important: Database Schema Changes

The following changes have been made to the database entities. You **MUST** create and run a migration to update your database schema.

### Changes Made

#### 1. User Entity - New Fields
- `SecondaryEmail` (string?, nullable, max 256)
- `InvoiceEmail` (string?, nullable, max 256)

#### 2. ClientProfile Entity - New Fields
- `ContactName` (string?, nullable, max 200)
- `Cell` (string?, nullable, max 20)
- `Fax` (string?, nullable, max 20)
- `State` (string?, nullable, max 100)
- `Website` (string?, nullable, max 500)
- `Reference` (string?, nullable, max 200)

### How to Create Migration

Run these commands in the `Backend/src/LogoDesignPortal.Infrastructure` directory:

```bash
# Navigate to Infrastructure project
cd Backend/src/LogoDesignPortal.Infrastructure

# Create migration
dotnet ef migrations add AddFullRegistrationFields --startup-project ../LogoDesignPortal.API

# Apply migration to database
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

### Migration Commands (Alternative)

If you're in the root directory:

```bash
# From project root
cd Backend/src/LogoDesignPortal.Infrastructure
dotnet ef migrations add AddFullRegistrationFields --startup-project ../LogoDesignPortal.API --context ApplicationDbContext
dotnet ef database update --startup-project ../LogoDesignPortal.API --context ApplicationDbContext
```

### What the Migration Will Do

1. **Add columns to Users table:**
   - `SecondaryEmail` (nvarchar(256), nullable)
   - `InvoiceEmail` (nvarchar(256), nullable)

2. **Add columns to ClientProfiles table:**
   - `ContactName` (nvarchar(200), nullable)
   - `Cell` (nvarchar(20), nullable)
   - `Fax` (nvarchar(20), nullable)
   - `State` (nvarchar(100), nullable)
   - `Website` (nvarchar(500), nullable)
   - `Reference` (nvarchar(200), nullable)

### ⚠️ Important Notes

- **Existing data will be preserved** - All new fields are nullable, so existing records won't be affected
- **No data loss** - This migration only adds new columns
- **Backup recommended** - Always backup your database before running migrations in production

### After Migration

Once the migration is complete:
1. ✅ All registration form fields will be saved
2. ✅ SuperAdmin can add complete information when creating users
3. ✅ Users can update all their information
4. ✅ Admins can view full user details

---

**Migration Status:** ⏳ **PENDING** - Must be run before using the new features
