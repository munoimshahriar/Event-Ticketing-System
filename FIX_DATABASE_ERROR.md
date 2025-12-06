# Fix Database Error: column e.OrganizerId does not exist

## Problem
The database schema is missing the new columns (`OrganizerId`, `UserId`) that were added to support Identity and user relationships.

## Quick Fix (Choose One)

### Option 1: Run SQL Script (Fastest - 2 minutes)
1. Open your PostgreSQL client (pgAdmin, DBeaver, or psql command line)
2. Connect to database: `ProjectManagementA`
3. Run the SQL script: `MIGRATION_SQL_SCRIPT.sql`
4. Restart your application
5. The application will automatically create Identity tables on first run

### Option 2: Use EF Core Migrations (Recommended - 5 minutes)
1. Open terminal in project root
2. Run:
   ```bash
   dotnet ef migrations add AddIdentityAndUserFields
   dotnet ef database update
   ```
3. This will:
   - Create Identity tables (AspNetUsers, AspNetRoles, etc.)
   - Add OrganizerId and UserId columns
   - Create EventRatings table
   - Add all foreign keys and indexes

### Option 3: Delete and Recreate (Loses Data - 1 minute)
**WARNING: This will delete all existing data!**

1. Delete the database:
   ```sql
   DROP DATABASE "ProjectManagementA";
   CREATE DATABASE "ProjectManagementA";
   ```
2. Restart your application
3. Everything will be created automatically

## Recommended: Option 2 (Migrations)

This is the best approach because:
- ✅ Preserves existing data
- ✅ Creates all Identity tables properly
- ✅ Tracks changes in version control
- ✅ Can be rolled back if needed

## After Fixing

Once the database is updated:
- ✅ The error will be resolved
- ✅ You can register/login users
- ✅ Events can be assigned to organizers
- ✅ Purchases can be linked to users

## Verification

After running the fix, verify by:
1. Check if `OrganizerId` column exists in `Events` table
2. Check if `UserId` column exists in `Purchases` table
3. Check if `AspNetUsers` table exists (Identity table)
4. Restart application - it should work without errors

