# Database Migration Instructions

## Option 1: Automatic Database Creation (Development)

The application is configured to automatically create the database and seed data on first run using `EnsureCreated()`. This is suitable for development.

Simply run:
```bash
dotnet run
```

The database will be created automatically if it doesn't exist.

## Option 2: Using Entity Framework Migrations (Recommended for Production)

### Step 1: Install EF Core Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

Or update if already installed:
```bash
dotnet tool update --global dotnet-ef
```

### Step 2: Create Initial Migration

```bash
dotnet ef migrations add InitialCreate
```

This will create a `Migrations` folder with the migration files.

### Step 3: Apply Migration to Database

```bash
dotnet ef database update
```

This will create the database and apply all migrations.

### Step 4: Update Program.cs (if using migrations)

If you want to use migrations instead of `EnsureCreated()`, update `Program.cs`:

**Remove or comment out:**
```csharp
context.Database.EnsureCreated();
```

**The migrations will handle database creation instead.**

## Option 3: Manual Database Setup

1. Create the database in PostgreSQL:
   ```sql
   CREATE DATABASE EventTicketingDB;
   ```

2. The application will create tables automatically on first run with `EnsureCreated()`.

## Verifying Database Creation

After running the application, you can verify the database was created by:

1. Connecting to PostgreSQL:
   ```bash
   psql -U postgres -d EventTicketingDB
   ```

2. List tables:
   ```sql
   \dt
   ```

3. Check seed data:
   ```sql
   SELECT * FROM "Categories";
   SELECT * FROM "Events";
   ```

## Troubleshooting

### Connection String Issues

Make sure your `appsettings.json` has the correct connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=EventTicketingDB;Username=postgres;Password=yourpassword"
}
```

### Migration Errors

If you encounter migration errors:
1. Delete the `Migrations` folder
2. Delete the database
3. Run `dotnet ef migrations add InitialCreate` again
4. Run `dotnet ef database update`

### EnsureCreated() vs Migrations

- **EnsureCreated()**: Creates database immediately, no migration history. Good for development.
- **Migrations**: Tracks database changes over time. Required for production and team collaboration.

