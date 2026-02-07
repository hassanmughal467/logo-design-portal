# Database Migration Guide

## Create Migration for New Entities

After adding the new entities (Message, Review, Settings), you need to create a migration:

```bash
cd Backend/src/LogoDesignPortal.Infrastructure
dotnet ef migrations add AddMessagesReviewsSettings --startup-project ../LogoDesignPortal.API
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

This will:
1. Create a migration file with the new entities
2. Update your database schema

## Verify Migration

After running the migration, verify that the new tables exist:
- Messages
- Reviews  
- Settings

---

**Note:** If you encounter any issues, you may need to delete the existing database and recreate it, or manually fix the migration.
