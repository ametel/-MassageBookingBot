# Starting All Services

This document contains the commands to start all services for the Massage Booking Bot application.

## Prerequisites

- .NET 10.0 SDK installed
- Node.js 24+ and npm installed
- All dependencies installed (`dotnet restore` and `npm install` in admin-panel)

## Start All Services

### 1. Start API Server

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot
dotnet run --project src/MassageBookingBot.Api/MassageBookingBot.Api.csproj
```

**Default URL:** http://localhost:5000

### 2. Start Bot Worker

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot
dotnet run --project src/MassageBookingBot.BotWorker/MassageBookingBot.BotWorker.csproj
```

**Bot:** @SiargukMassageBot

### 3. Start Admin Panel

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot\admin-panel
npm start
```

**Default URL:** http://localhost:4200

## Quick Start (All Services in Separate Terminals)

Open three PowerShell terminals and run each command in a separate terminal:

**Terminal 1 - API:**

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot; dotnet run --project src/MassageBookingBot.Api/MassageBookingBot.Api.csproj
```

**Terminal 2 - Bot:**

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot; dotnet run --project src/MassageBookingBot.BotWorker/MassageBookingBot.BotWorker.csproj
```

**Terminal 3 - Admin Panel:**

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot\admin-panel; npm start
```

## Verify Services

Check if all services are running:

```powershell
# Check API (should return health status)
Invoke-RestMethod -Uri "http://localhost:5000/health"

# Check listening ports
netstat -ano | Select-String "LISTENING" | Select-String ":5000|:4200"
```

## Stopping Services

Press `Ctrl+C` in each terminal to stop the respective service.

## Troubleshooting

### API won't start

- Check if port 5000 is already in use
- Ensure database file exists: `src/MassageBookingBot.Api/massagebooking.db`
- Check Telegram bot token is set in user secrets

### Bot Worker won't start

- Verify Telegram bot token in `appsettings.json` or user secrets
- Check bot token validity with BotFather

### Admin Panel won't start

- **IMPORTANT:** Make sure you're in the `admin-panel` directory before running `npm start`
  - ❌ Wrong: Running from project root will give "ENOENT: package.json not found" error
  - ✅ Correct: `cd admin-panel` first, then run `npm start`
- Run `npm install` in the admin-panel directory
- Check if port 4200 is available
- Ensure Node.js version is 20.19+ or 22.12+

### Admin Panel shows no bookings

- Verify API is running on http://localhost:5000
- Check browser console for CORS errors
- Ensure bookings exist in the database
- Try refreshing the page (Ctrl+R)
