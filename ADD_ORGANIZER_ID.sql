-- Add OrganizerId column to Events table if it doesn't exist
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
        
        CREATE INDEX IF NOT EXISTS "IX_Events_OrganizerId" 
        ON "Events" ("OrganizerId");
        
        ALTER TABLE "Events"
        ADD CONSTRAINT "FK_Events_AspNetUsers_OrganizerId"
        FOREIGN KEY ("OrganizerId")
        REFERENCES "AspNetUsers" ("Id")
        ON DELETE SET NULL;
    END IF;
END $$;

