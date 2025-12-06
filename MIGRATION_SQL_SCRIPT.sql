-- Migration script to add Identity and new user relationship columns
-- Run this script on your PostgreSQL database
-- IMPORTANT: Run this BEFORE running the application with Identity

-- Step 1: Add OrganizerId to Events table (nullable)
ALTER TABLE "Events" ADD COLUMN IF NOT EXISTS "OrganizerId" TEXT NULL;

-- Step 2: Add UserId to Purchases table (nullable)
ALTER TABLE "Purchases" ADD COLUMN IF NOT EXISTS "UserId" TEXT NULL;

-- Step 3: Create indexes (foreign keys will be added after Identity tables are created)
CREATE INDEX IF NOT EXISTS "IX_Events_OrganizerId" ON "Events" ("OrganizerId");
CREATE INDEX IF NOT EXISTS "IX_Purchases_UserId" ON "Purchases" ("UserId");

-- Step 4: Create EventRatings table (without foreign key to AspNetUsers yet)
CREATE TABLE IF NOT EXISTS "EventRatings" (
    "Id" SERIAL PRIMARY KEY,
    "PurchaseItemId" INTEGER NOT NULL,
    "UserId" TEXT NOT NULL,
    "Rating" INTEGER NOT NULL CHECK ("Rating" >= 1 AND "Rating" <= 5),
    "Comment" TEXT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    CONSTRAINT "FK_EventRatings_PurchaseItems_PurchaseItemId" 
        FOREIGN KEY ("PurchaseItemId") REFERENCES "PurchaseItems" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_EventRatings_PurchaseItemId" ON "EventRatings" ("PurchaseItemId");
CREATE INDEX IF NOT EXISTS "IX_EventRatings_UserId" ON "EventRatings" ("UserId");

-- Note: After running this script, restart your application.
-- The application will create Identity tables (AspNetUsers, AspNetRoles, etc.) automatically.
-- Then you can add the foreign key constraints manually if needed, or use EF migrations.

