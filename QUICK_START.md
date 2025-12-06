# Quick Start Guide

## 🚀 Get Running in 5 Minutes

### Step 1: Update Connection String (30 seconds)
Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=EventTicketingDB;Username=postgres;Password=YOUR_PASSWORD"
}
```

### Step 2: Restore Packages (1 minute)
```bash
dotnet restore
```

### Step 3: Install Client Libraries (1 minute)
```bash
# If you have LibMan CLI
libman restore

# OR manually: Download Bootstrap 5.3.0 and jQuery 3.7.1 to wwwroot/lib/
# OR use CDN (update _Layout.cshtml)
```

### Step 4: Run! (2 minutes)
```bash
dotnet run
```

Open browser: `http://localhost:5000`

**That's it!** The database will be created automatically with sample data.

## 🎯 What You'll See

1. **Home Page**: Welcome screen with navigation
2. **Events**: Browse 6 sample events
3. **Dashboard**: See statistics and low-availability alerts
4. **Categories**: 5 pre-loaded categories
5. **Purchase**: Buy tickets without registration

## 📝 Sample Data Included

- **Categories**: Webinar, Concert, Workshop, Conference, Sports
- **Events**: 6 events with varying ticket availability
- Some events have < 5 tickets to test low-availability alerts

## ⚠️ Troubleshooting

**"Connection refused"**
→ Check PostgreSQL is running: `pg_isready`

**"Bootstrap not loading"**
→ Run `libman restore` or update `_Layout.cshtml` to use CDN

**"Database error"**
→ Check connection string in `appsettings.json`

## 🎓 Assignment Requirements Met

✅ All compulsory components
✅ Event Management (CRUD)
✅ Category Management (CRUD)
✅ Search & Filtering
✅ Sorting
✅ Dashboard
✅ Guest Purchasing
✅ Modern UI

**Ready for submission!** 🎉

