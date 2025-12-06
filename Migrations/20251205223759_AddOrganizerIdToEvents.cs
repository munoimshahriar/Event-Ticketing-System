using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualEventTicketing.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizerIdToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if column exists before adding (for cases where database is out of sync)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'Events' 
                        AND column_name = 'OrganizerId'
                    ) THEN
                        ALTER TABLE ""Events"" 
                        ADD COLUMN ""OrganizerId"" text NULL;
                    END IF;
                END $$;
            ");

            // Create index if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM pg_indexes 
                        WHERE tablename = 'Events' 
                        AND indexname = 'IX_Events_OrganizerId'
                    ) THEN
                        CREATE INDEX ""IX_Events_OrganizerId"" ON ""Events"" (""OrganizerId"");
                    END IF;
                END $$;
            ");

            // Add foreign key constraint if it doesn't exist
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.table_constraints 
                        WHERE constraint_name = 'FK_Events_AspNetUsers_OrganizerId'
                    ) THEN
                        ALTER TABLE ""Events""
                        ADD CONSTRAINT ""FK_Events_AspNetUsers_OrganizerId""
                        FOREIGN KEY (""OrganizerId"")
                        REFERENCES ""AspNetUsers"" (""Id"")
                        ON DELETE SET NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_AspNetUsers_OrganizerId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_OrganizerId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "OrganizerId",
                table: "Events");
        }
    }
}
