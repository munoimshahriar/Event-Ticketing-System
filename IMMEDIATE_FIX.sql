-- IMMEDIATE FIX - Run this SQL script NOW to fix the error
-- Copy and paste this into your PostgreSQL client (pgAdmin, DBeaver, or psql)

-- Connect to your database first:
-- \c ProjectManagementA

-- Add the missing columns
ALTER TABLE "Events" ADD COLUMN IF NOT EXISTS "OrganizerId" TEXT NULL;
ALTER TABLE "Purchases" ADD COLUMN IF NOT EXISTS "UserId" TEXT NULL;

-- Create indexes
CREATE INDEX IF NOT EXISTS "IX_Events_OrganizerId" ON "Events" ("OrganizerId");
CREATE INDEX IF NOT EXISTS "IX_Purchases_UserId" ON "Purchases" ("UserId");

-- That's it! Restart your application after running this.

