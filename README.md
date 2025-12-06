# Virtual Event Ticketing System

A web-based Virtual Event Ticketing System built with ASP.NET Core MVC for managing virtual events, categories, and guest ticket purchases.

## Features

- **Event Management**: Create, read, update, and delete events with categories
- **Category Management**: Manage event categories (Webinar, Concert, Workshop, Conference, Sports)
- **Search & Filtering**: Search events by title, filter by category, date range, and availability status
- **Sorting**: Sort events by title, date, or price
- **Dashboard**: View event statistics and low-availability alerts
- **Guest Ticket Purchasing**: Allow guests to purchase tickets without authentication
- **Purchase Confirmation**: Detailed purchase summary with event information

## Technologies

- ASP.NET Core MVC 8.0
- Entity Framework Core 8.0
- PostgreSQL (via Npgsql)
- Bootstrap 5 (for UI)
- C#

## Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL database server
- Visual Studio 2022, VS Code, or Rider (optional)

## Setup Instructions

### 1. Database Setup

1. Install PostgreSQL if not already installed
2. Create a new database:
   ```sql
   CREATE DATABASE EventTicketingDB;
   ```
3. Update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=EventTicketingDB;Username=postgres;Password=yourpassword"
   }
   ```

### 2. Install Dependencies

Restore NuGet packages:
```bash
dotnet restore
```

### 3. Database Migrations

Create and apply the initial migration:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Note**: If you don't have the EF Core tools installed globally, install them first:
```bash
dotnet tool install --global dotnet-ef
```

Alternatively, the application will automatically create the database and seed initial data on first run using `EnsureCreated()`.

### 4. Run the Application

```bash
dotnet run
```

Or use your IDE's run command. The application will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## Project Structure

```
VirtualEventTicketing/
├── Controllers/
│   ├── CategoriesController.cs
│   ├── EventsController.cs
│   ├── HomeController.cs
│   └── PurchasesController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Seeders/
│       └── DatabaseSeeder.cs
├── Models/
│   ├── Category.cs
│   ├── Event.cs
│   ├── Purchase.cs
│   ├── PurchaseItem.cs
│   ├── EventFilterViewModel.cs
│   └── PurchaseViewModel.cs
├── Views/
│   ├── Categories/
│   ├── Events/
│   ├── Home/
│   ├── Purchases/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   └── js/
├── appsettings.json
├── Program.cs
└── VirtualEventTicketing.csproj
```

## Database Schema

### Tables

- **Categories**: Event categories (Id, Name, Description)
- **Events**: Virtual events (Id, Title, Date, TicketPrice, AvailableTickets, CategoryId)
- **Purchases**: Guest purchases (Id, PurchaseDate, TotalCost, GuestName, GuestEmail)
- **PurchaseItems**: Purchase details (Id, PurchaseId, EventId, Quantity)

### Relationships

- Category → Events: One-to-Many
- Event → PurchaseItems: One-to-Many
- Purchase → PurchaseItems: One-to-Many
- Events ↔ Purchases: Many-to-Many (through PurchaseItems)

## Seed Data

The application includes seed data with:
- 5 Categories (Webinar, Concert, Workshop, Conference, Sports)
- 6 Sample Events across different categories

## Usage

1. **Home Page**: Navigate to the homepage to see an overview and quick links
2. **Events**: Browse all events, use search and filters to find specific events
3. **Dashboard**: View statistics and events with low ticket availability
4. **Categories**: Manage event categories
5. **Purchase Tickets**: Click "Buy Tickets" on any available event to purchase

## Assignment Requirements Met

✅ Event Management (CRUD operations)
✅ Category Management (CRUD operations)
✅ Search and Filtering (by title, category, date range, availability)
✅ Sorting (by title, date, price)
✅ Event Dashboard with statistics
✅ Guest Ticket Purchasing
✅ Purchase Confirmation Page
✅ Responsive UI with Bootstrap
✅ Navigation Bar and Footer
✅ Data Validation
✅ Entity Framework Core with PostgreSQL

## Notes

- The application uses `EnsureCreated()` for initial database setup, which is suitable for development
- For production, use proper migrations: `dotnet ef migrations add` and `dotnet ef database update`
- Update the PostgreSQL connection string in `appsettings.json` before running
- Seed data is automatically added on first run

## License

This project is created for COMP2139 - Assignment 1.

