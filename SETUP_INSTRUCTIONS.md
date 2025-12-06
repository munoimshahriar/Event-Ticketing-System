# Quick Setup Instructions

## Prerequisites Check

1. ✅ .NET 8.0 SDK installed
   ```bash
   dotnet --version
   ```
   Should show version 8.0.x or higher

2. ✅ PostgreSQL installed and running
   ```bash
   psql --version
   ```

## Step-by-Step Setup

### 1. Update Database Connection String

Edit `appsettings.json` and update the connection string with your PostgreSQL credentials:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=EventTicketingDB;Username=postgres;Password=YOUR_PASSWORD"
}
```

### 2. Create PostgreSQL Database (Optional)

If you want to create the database manually:

```bash
psql -U postgres
```

Then run:
```sql
CREATE DATABASE EventTicketingDB;
\q
```

**Note**: The application will create the database automatically if it doesn't exist (using `EnsureCreated()`).

### 3. Restore NuGet Packages

```bash
dotnet restore
```

### 4. Install Client-Side Libraries (Bootstrap, jQuery)

If you have LibMan installed:
```bash
libman restore
```

Or manually download and place:
- Bootstrap 5.3.0 → `wwwroot/lib/bootstrap/`
- jQuery 3.7.1 → `wwwroot/lib/jquery/`
- jQuery Validation → `wwwroot/lib/jquery-validation/`
- jQuery Validation Unobtrusive → `wwwroot/lib/jquery-validation-unobtrusive/`

**Alternative**: Use CDN links in `_Layout.cshtml` if local files aren't available.

### 5. Run the Application

```bash
dotnet run
```

Or press F5 in Visual Studio.

The application will:
- Create the database automatically (if it doesn't exist)
- Create all tables
- Seed sample data (5 categories, 6 events)

### 6. Access the Application

Open your browser and navigate to:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## Using Entity Framework Migrations (Optional)

If you prefer to use migrations instead of `EnsureCreated()`:

1. Install EF Core tools:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Create migration:
   ```bash
   dotnet ef migrations add InitialCreate
   ```

3. Apply migration:
   ```bash
   dotnet ef database update
   ```

4. Update `Program.cs` to remove `EnsureCreated()` line (line 41)

## Troubleshooting

### "Connection refused" or "Database does not exist"

- Check PostgreSQL is running: `pg_isready`
- Verify connection string in `appsettings.json`
- Ensure database exists or let the app create it automatically

### "Package not found" errors

- Run `dotnet restore`
- Check internet connection for NuGet package downloads

### Bootstrap/jQuery not loading

- Run `libman restore` to install client libraries
- Or update `_Layout.cshtml` to use CDN links instead

### Migration errors

- Delete `Migrations` folder if it exists
- Delete the database
- Let the app create it automatically with `EnsureCreated()`

## Testing the Application

1. **Home Page**: Should show welcome message and navigation
2. **Events**: Browse events, test search and filters
3. **Dashboard**: View statistics and low-availability alerts
4. **Categories**: Create, edit, delete categories
5. **Purchase**: Buy tickets for an event (no authentication required)

## Sample Data

The application seeds:
- **5 Categories**: Webinar, Concert, Workshop, Conference, Sports
- **6 Events**: Various events across different categories with different ticket availability

## Next Steps

- Customize the UI/UX
- Add more features
- Prepare for Assignment 2 (user authentication, etc.)

