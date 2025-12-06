-- Fix: Add OrganizerId column to Events table if it doesn't exist
-- Run this script directly in your PostgreSQL database

-- Add the column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_name = 'Events' 
        AND column_name = 'OrganizerId'
    ) THEN
        ALTER TABLE "Events" 
        ADD COLUMN "OrganizerId" text NULL;
        
        RAISE NOTICE 'Added OrganizerId column to Events table';
    ELSE
        RAISE NOTICE 'OrganizerId column already exists';
    END IF;
END $$;

-- Create index if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 
        FROM pg_indexes 
        WHERE tablename = 'Events' 
        AND indexname = 'IX_Events_OrganizerId'
    ) THEN
        CREATE INDEX "IX_Events_OrganizerId" ON "Events" ("OrganizerId");
        
        RAISE NOTICE 'Created index IX_Events_OrganizerId';
    ELSE
        RAISE NOTICE 'Index IX_Events_OrganizerId already exists';
    END IF;
END $$;

-- Add foreign key constraint if it doesn't exist and AspNetUsers table exists
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetUsers') THEN
        IF NOT EXISTS (
            SELECT 1 
            FROM information_schema.table_constraints 
            WHERE constraint_name = 'FK_Events_AspNetUsers_OrganizerId'
        ) THEN
            ALTER TABLE "Events"
            ADD CONSTRAINT "FK_Events_AspNetUsers_OrganizerId"
            FOREIGN KEY ("OrganizerId")
            REFERENCES "AspNetUsers" ("Id")
            ON DELETE SET NULL;
            
            RAISE NOTICE 'Added foreign key constraint FK_Events_AspNetUsers_OrganizerId';
        ELSE
            RAISE NOTICE 'Foreign key constraint FK_Events_AspNetUsers_OrganizerId already exists';
        END IF;
    ELSE
        RAISE NOTICE 'AspNetUsers table does not exist yet - foreign key will be added when Identity tables are created';
    END IF;
END $$;

