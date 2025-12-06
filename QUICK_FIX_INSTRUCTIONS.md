# Quick Fix for Database Schema Error

## Problem
The database is missing the new columns (`OrganizerId`, `UserId`) and Identity tables that were added to the models.

## Solution Options

### Option 1: Run SQL Script (Recommended - Preserves Data)
1. Open your PostgreSQL database (pgAdmin, DBeaver, or psql)
2. Connect to your database: `ProjectManagementA`
3. Run the SQL script: `MIGRATION_SQL_SCRIPT.sql`
4. Restart your application

### Option 2: Delete and Recreate Database (Loses All Data)
1. Delete the database:
   ```sql
   DROP DATABASE "ProjectManagementA";
   CREATE DATABASE "ProjectManagementA";
   ```
2. Restart your application - it will auto-create all tables including Identity tables

### Option 3: Use Entity Framework Migrations (Best Practice)
1. Install EF Core tools (if not installed):
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Create a new migration:
   ```bash
   dotnet ef migrations add AddIdentityAndUserFields
   ```

3. Apply the migration:
   ```bash
   dotnet ef database update
   ```

## Recommended: Option 3 (Migrations)

This is the best approach as it:
- Tracks all changes
- Can be version controlled
- Allows rollback if needed
- Works well with Identity

After running the migration, your database will have:
- All Identity tables (AspNetUsers, AspNetRoles, etc.)
- OrganizerId column in Events
- UserId column in Purchases
- EventRatings table

