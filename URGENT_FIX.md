# 🚨 URGENT FIX - Run This Now!

## The Problem
Your database is missing the `OrganizerId` and `UserId` columns that were added to the models.

## ⚡ Quickest Fix (30 seconds)

### Step 1: Open PostgreSQL
- Open pgAdmin, DBeaver, or psql command line
- Connect to your database: `ProjectManagementA`

### Step 2: Run This SQL
Copy and paste this into your SQL editor:

```sql
ALTER TABLE "Events" ADD COLUMN IF NOT EXISTS "OrganizerId" TEXT NULL;
ALTER TABLE "Purchases" ADD COLUMN IF NOT EXISTS "UserId" TEXT NULL;
CREATE INDEX IF NOT EXISTS "IX_Events_OrganizerId" ON "Events" ("OrganizerId");
CREATE INDEX IF NOT EXISTS "IX_Purchases_UserId" ON "Purchases" ("UserId");
```

### Step 3: Restart Application
Restart your application - the error will be gone!

## ✅ Verification
After running the SQL, verify it worked:
```sql
SELECT column_name FROM information_schema.columns 
WHERE table_name = 'Events' AND column_name = 'OrganizerId';
```

Should return: `OrganizerId`

## 📝 Alternative: Use EF Migrations (Better long-term)

If you prefer using migrations (recommended):

```bash
dotnet ef migrations add AddIdentityAndUserFields
dotnet ef database update
```

But the SQL script above is faster if you just need it working now!

