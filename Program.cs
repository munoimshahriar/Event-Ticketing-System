using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.Data.Seeders;
using VirtualEventTicketing.Models;
using VirtualEventTicketing.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Creates daily log files
    .CreateLogger();

builder.Host.UseSerilog(); // Tells the app to use Serilog instead of default logger

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support for shopping cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Npgsql to treat unspecified DateTime as UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add DbContext with Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = true; // Email confirmation is now required

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("OrganizerOnly", policy => policy.RequireRole("Organizer"));
    options.AddPolicy("AttendeeOnly", policy => policy.RequireRole("Attendee"));
    options.AddPolicy("OrganizerOrAdmin", policy => 
        policy.RequireAssertion(context => 
            context.User.IsInRole("Organizer") || context.User.IsInRole("Admin")));
});

// Configure Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// Add this line to register your custom email sender
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, VirtualEventTicketing.Services.EmailSender>();

// Register services
builder.Services.AddScoped<VirtualEventTicketing.Services.QRCodeService>();
builder.Services.AddScoped<VirtualEventTicketing.Services.PDFTicketService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

// Global exception handling
app.UseGlobalExceptionHandling();

// 404 handling
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        context.Request.Path = "/Home/Error404";
        await next();
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Log authorization failures
app.UseAuthorizationFailureLogging();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Use migrations instead of EnsureCreated for Identity support
        // If database doesn't exist, migrations will create it
        try
        {
            context.Database.Migrate();
        }
        catch (Exception migrateEx)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(migrateEx, "Migration failed, attempting to create missing tables manually.");
        }
        
        // Always check if Identity tables exist (in case migrations are out of sync)
        await EnsureIdentityTablesExistAsync(context);

        // Ensure OrganizerId column exists (for databases that were created before this column was added)
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
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
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Could not add OrganizerId column - it may already exist or database connection failed.");
        }

        // Remove FullName column if it exists (for databases that were created before this column was removed)
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'AspNetUsers' 
                        AND column_name = 'FullName'
                    ) THEN
                        ALTER TABLE ""AspNetUsers"" 
                        DROP COLUMN ""FullName"";
                    END IF;
                END $$;
            ");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Could not remove FullName column - it may not exist or database connection failed.");
        }
        
        // Seed database (roles, admin user, categories, events)
        await DatabaseSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        Log.Error(ex, "An error occurred while seeding the database.");
    }
}

app.Run();

// Helper methods
async Task EnsureIdentityTablesExistAsync(ApplicationDbContext context)
{
    try
    {
        // Check if AspNetUsers exists using a simple query
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        try
        {
            using var command = connection.CreateCommand();
            
            // Check if AspNetUsers exists
            command.CommandText = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'AspNetUsers');";
            var exists = (bool)(await command.ExecuteScalarAsync())!;
            
            if (!exists)
            {
                // Apply the Identity migration manually
                await ApplyIdentityMigrationAsync(context);
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    catch (Exception ex)
    {
        // Log error but don't throw - let the app continue
        // Note: Using Serilog static logger here since we don't have ILogger in this scope
        Log.Warning(ex, "Failed to ensure Identity tables exist");
    }
}

async Task ApplyIdentityMigrationAsync(ApplicationDbContext context)
{
    // Apply the Identity migration SQL to create all Identity tables
    var migrationSql = @"
        -- Create AspNetRoles table
        CREATE TABLE IF NOT EXISTS ""AspNetRoles"" (
            ""Id"" text NOT NULL,
            ""Name"" character varying(256),
            ""NormalizedName"" character varying(256),
            ""ConcurrencyStamp"" text,
            CONSTRAINT ""PK_AspNetRoles"" PRIMARY KEY (""Id"")
        );

        CREATE UNIQUE INDEX IF NOT EXISTS ""RoleNameIndex"" ON ""AspNetRoles"" (""NormalizedName"");

        -- Create AspNetUsers table
        CREATE TABLE IF NOT EXISTS ""AspNetUsers"" (
            ""Id"" text NOT NULL,
            ""PhoneNumber"" character varying(20),
            ""DateOfBirth"" timestamp without time zone,
            ""ProfilePictureUrl"" character varying(500),
            ""UserName"" character varying(256),
            ""NormalizedUserName"" character varying(256),
            ""Email"" character varying(256),
            ""NormalizedEmail"" character varying(256),
            ""EmailConfirmed"" boolean NOT NULL,
            ""PasswordHash"" text,
            ""SecurityStamp"" text,
            ""ConcurrencyStamp"" text,
            ""PhoneNumberConfirmed"" boolean NOT NULL,
            ""TwoFactorEnabled"" boolean NOT NULL,
            ""LockoutEnd"" timestamp with time zone,
            ""LockoutEnabled"" boolean NOT NULL,
            ""AccessFailedCount"" integer NOT NULL,
            CONSTRAINT ""PK_AspNetUsers"" PRIMARY KEY (""Id"")
        );

        CREATE INDEX IF NOT EXISTS ""EmailIndex"" ON ""AspNetUsers"" (""NormalizedEmail"");
        CREATE UNIQUE INDEX IF NOT EXISTS ""UserNameIndex"" ON ""AspNetUsers"" (""NormalizedUserName"");

        -- Create other Identity tables
        CREATE TABLE IF NOT EXISTS ""AspNetRoleClaims"" (
            ""Id"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
            ""RoleId"" text NOT NULL,
            ""ClaimType"" text,
            ""ClaimValue"" text,
            CONSTRAINT ""PK_AspNetRoleClaims"" PRIMARY KEY (""Id""),
            CONSTRAINT ""FK_AspNetRoleClaims_AspNetRoles_RoleId"" FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles"" (""Id"") ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS ""IX_AspNetRoleClaims_RoleId"" ON ""AspNetRoleClaims"" (""RoleId"");

        CREATE TABLE IF NOT EXISTS ""AspNetUserClaims"" (
            ""Id"" integer NOT NULL GENERATED BY DEFAULT AS IDENTITY,
            ""UserId"" text NOT NULL,
            ""ClaimType"" text,
            ""ClaimValue"" text,
            CONSTRAINT ""PK_AspNetUserClaims"" PRIMARY KEY (""Id""),
            CONSTRAINT ""FK_AspNetUserClaims_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS ""IX_AspNetUserClaims_UserId"" ON ""AspNetUserClaims"" (""UserId"");

        CREATE TABLE IF NOT EXISTS ""AspNetUserLogins"" (
            ""LoginProvider"" text NOT NULL,
            ""ProviderKey"" text NOT NULL,
            ""ProviderDisplayName"" text,
            ""UserId"" text NOT NULL,
            CONSTRAINT ""PK_AspNetUserLogins"" PRIMARY KEY (""LoginProvider"", ""ProviderKey""),
            CONSTRAINT ""FK_AspNetUserLogins_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS ""IX_AspNetUserLogins_UserId"" ON ""AspNetUserLogins"" (""UserId"");

        CREATE TABLE IF NOT EXISTS ""AspNetUserRoles"" (
            ""UserId"" text NOT NULL,
            ""RoleId"" text NOT NULL,
            CONSTRAINT ""PK_AspNetUserRoles"" PRIMARY KEY (""UserId"", ""RoleId""),
            CONSTRAINT ""FK_AspNetUserRoles_AspNetRoles_RoleId"" FOREIGN KEY (""RoleId"") REFERENCES ""AspNetRoles"" (""Id"") ON DELETE CASCADE,
            CONSTRAINT ""FK_AspNetUserRoles_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS ""IX_AspNetUserRoles_RoleId"" ON ""AspNetUserRoles"" (""RoleId"");

        CREATE TABLE IF NOT EXISTS ""AspNetUserTokens"" (
            ""UserId"" text NOT NULL,
            ""LoginProvider"" text NOT NULL,
            ""Name"" text NOT NULL,
            ""Value"" text,
            CONSTRAINT ""PK_AspNetUserTokens"" PRIMARY KEY (""UserId"", ""LoginProvider"", ""Name""),
            CONSTRAINT ""FK_AspNetUserTokens_AspNetUsers_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""AspNetUsers"" (""Id"") ON DELETE CASCADE
        );
    ";

    await context.Database.ExecuteSqlRawAsync(migrationSql);
}

