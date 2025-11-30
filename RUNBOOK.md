# Massage Booking Bot - Quick Runbook

## Current Status

### ✅ Successfully Running

- **API Server**: http://localhost:5000
  - Swagger UI: http://localhost:5000/swagger
  - Database: SQLite (`massagebooking.db`) created with sample data
  - Quartz.NET scheduler: Active (runs every 30 minutes)

### ⚠️ Configuration Needed

1. **Telegram Bot Token**: Empty in `appsettings.json`
2. **Node.js**: v18.12.1 installed, but admin panel requires v20.19+

---

## Quick Start Commands

### Start API Server

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api
dotnet run
```

**URL**: http://localhost:5000

### Start Bot Worker (After configuring token)

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.BotWorker
dotnet run
```

### Start Admin Panel (After upgrading Node.js)

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot\admin-panel
npm start
```

**URL**: http://localhost:4200

---

## Test API Endpoints

### Get All Services

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/services" -Method GET | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

### Create a Booking

```powershell
$booking = @{
    userId = 1
    serviceId = 1
    bookingDateTime = "2025-12-05T10:00:00Z"
    notes = "Test booking"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/api/bookings" `
    -Method POST `
    -ContentType "application/json" `
    -Body $booking
```

### Get All Bookings

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/bookings" -Method GET | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

---

## Configuration Steps

### 1. Configure Telegram Bot Token

**Get Token**:

1. Open Telegram and search for `@BotFather`
2. Send `/newbot` and follow instructions
3. Copy the token provided

**Update Configuration**:

File: `src/MassageBookingBot.Api/appsettings.json`

```json
{
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN_HERE"
  }
}
```

File: `src/MassageBookingBot.BotWorker/appsettings.json`

```json
{
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN_HERE"
  }
}
```

**Restart Services**:

- Stop API and Bot Worker (Ctrl+C)
- Restart both services

### 2. Upgrade Node.js (For Admin Panel)

**Download Node.js v20 LTS**:

- Visit: https://nodejs.org/
- Download v20.x LTS version
- Install and restart terminal

**Verify Installation**:

```powershell
node --version  # Should show v20.x or v22.x
```

**Run Admin Panel**:

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot\admin-panel
npm install
npm start
```

---

## Useful URLs

- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Admin Panel**: http://localhost:4200 (after Node.js upgrade)

---

## Database Management

### Location

```
K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api\massagebooking.db
```

### View Database

**Option 1**: SQLite Browser (https://sqlitebrowser.org/)

**Option 2**: PowerShell SQLite Module

```powershell
# Install if needed
Install-Module -Name PSSQLite -Force

# Query database
$dbPath = "K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api\massagebooking.db"
Invoke-SqliteQuery -DataSource $dbPath -Query "SELECT * FROM Services"
```

### Reset Database

**Delete and restart API** (database will be recreated with seed data):

```powershell
Remove-Item K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api\massagebooking.db
cd K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api
dotnet run
```

---

## Seeded Data

### Services (5 total)

1. Swedish Massage - $80, 60 min
2. Deep Tissue Massage - $100, 75 min
3. Hot Stone Massage - $120, 90 min
4. Sports Massage - $90, 60 min
5. Aromatherapy Massage - $110, 75 min

### Time Slots (56 total)

- 7 days starting from today
- 8 slots per day (9:00 AM - 5:00 PM)
- 1-hour intervals

---

## Troubleshooting

### API Won't Start

**Check**:

- Port 5000 not already in use
- Database file permissions
- Valid appsettings.json format

**Solution**:

```powershell
# Check port usage
netstat -ano | findstr :5000

# Stop process if needed
Stop-Process -Id <PID>
```

### Bot Worker Crashes

**Most likely**: Missing or invalid Telegram Bot Token

**Check logs** for error message

**Solution**: Configure valid bot token in appsettings.json

### Admin Panel Build Errors

**Most likely**: Node.js version too old

**Solution**: Upgrade to Node.js v20+

### Database Errors

**Solution**: Delete database file and restart API to recreate

---

## Next Steps

1. ✅ **API is running** - Test endpoints via Swagger
2. ⚠️ **Configure Telegram Bot** - Get token from BotFather
3. ⚠️ **Upgrade Node.js** - Install v20+ for admin panel
4. 📋 **Follow Testing Plan** - See TESTING_PLAN.md
5. 🚀 **Test Complete Flow** - Bot → Booking → Notifications → Admin Panel

---

## Support Files

- **README.md** - Full project documentation
- **SETUP.md** - Detailed setup instructions
- **TESTING_PLAN.md** - Comprehensive testing guide
- **IMPLEMENTATION_SUMMARY.md** - Technical implementation details

---

_Last Updated: 2025-11-30_
