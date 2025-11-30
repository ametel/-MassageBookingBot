# 📍 Quick Reference - Log Locations & Solutions

## Current Log Locations

### ✅ **Active: Console Logs**

Logs appear in real-time in the terminals where services are running.

#### API Server Terminal

```powershell
Location: K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api
Command: dotnet run
```

**Shows**: API requests, database operations, scheduler events

#### Bot Worker Terminal

```powershell
Location: K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.BotWorker
Command: dotnet run
```

**Shows**: Telegram updates, user interactions, bot responses

---

## ⚠️ **Planned: File Logs**

File logging is **configured but requires Serilog** package installation.

### Once Serilog is installed, logs will be at:

| Service    | Log Location                                                  | Retention |
| ---------- | ------------------------------------------------------------- | --------- |
| API        | `src/MassageBookingBot.Api/logs/api-YYYYMMDD.log`             | 7 days    |
| Bot Worker | `src/MassageBookingBot.BotWorker/logs/botworker-YYYYMMDD.log` | 7 days    |

### Install Serilog:

```powershell
# API
cd src\MassageBookingBot.Api
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File

# Bot Worker
cd ..\MassageBookingBot.BotWorker
dotnet add package Serilog.Extensions.Hosting
dotnet add package Serilog.Sinks.File
```

---

## 🔍 Quick Troubleshooting

### Problem: Bot not responding

**Check console for**:

```
fail: Error in Telegram Bot
fail: Telegram Bot API error 409
```

**Solutions**:

```powershell
# Stop all instances
Get-Process -Name dotnet | Stop-Process -Force

# Restart
cd src\MassageBookingBot.BotWorker
dotnet run
```

---

### Problem: Database errors

**Check console for**:

```
SQLite Error 1: 'no such table: Users'
database is locked
```

**Solutions**:

```powershell
# Verify database location
dir src\MassageBookingBot.Api\massagebooking.db

# Check Bot Worker config points to API's database
# In appsettings.json:
"DefaultConnection": "Data Source=..\\MassageBookingBot.Api\\massagebooking.db"

# If needed, delete and recreate
Remove-Item src\MassageBookingBot.Api\massagebooking.db
# Restart API to recreate
```

---

### Problem: API won't start

**Check console for**:

```
Port already in use
Failed to bind to address
```

**Solutions**:

```powershell
# Check what's using port 5000
netstat -ano | findstr :5000

# Stop the process
Get-Process -Id <PID> | Stop-Process -Force

# Restart API
cd src\MassageBookingBot.Api
dotnet run
```

---

## 📊 Log Levels (Current)

### API Server

```json
"Default": "Information"
"MassageBookingBot": "Debug"
"Microsoft.EntityFrameworkCore.Database.Command": "Warning"
```

### Bot Worker

```json
"Default": "Information"
"MassageBookingBot": "Debug"
"Microsoft.EntityFrameworkCore.Database.Command": "Warning"
```

**Change in**: `appsettings.json` files

---

## 🎯 What Each Service Logs

### API Server Logs:

- ✅ HTTP requests (GET, POST, DELETE)
- ✅ API endpoint execution
- ✅ Database operations
- ✅ Quartz.NET scheduler events
- ✅ Booking reminders sent
- ✅ Errors and exceptions

### Bot Worker Logs:

- ✅ Telegram updates received
- ✅ User commands (/start, /book, etc.)
- ✅ Message processing with user IDs
- ✅ Service selections
- ✅ Booking flow steps
- ✅ Database queries
- ✅ Bot responses sent
- ✅ Errors and exceptions

---

## 🔧 Essential Commands

### View Running Services

```powershell
Get-Process -Name dotnet
```

### Stop All Services

```powershell
Get-Process -Name dotnet | Stop-Process -Force
```

### Restart Services Clean

```powershell
# Stop all
Get-Process -Name dotnet | Stop-Process -Force

# Start API (Terminal 1)
cd K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api
dotnet run

# Start Bot Worker (Terminal 2)
cd K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.BotWorker
dotnet run
```

### Check Database Location

```powershell
dir K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api\massagebooking.db
```

---

## 📝 Log Configuration Files

### API

```
K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api\appsettings.json
```

### Bot Worker

```
K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.BotWorker\appsettings.json
```

---

## 🚨 Common Error Patterns

| Error Message                              | Cause                    | Solution                      |
| ------------------------------------------ | ------------------------ | ----------------------------- |
| `no such table: Users`                     | Database not initialized | Delete DB, restart API        |
| `Conflict: terminated by other getUpdates` | Multiple bot instances   | Stop all, restart one         |
| `Failed to bind to address`                | Port already in use      | Kill process on port 5000     |
| `Telegram Bot Token not configured`        | Empty token in config    | Add token to appsettings.json |
| `database is locked`                       | Concurrent access issue  | Restart both services         |
| `Cannot access a disposed object`          | Service lifecycle issue  | Restart affected service      |

---

## 📖 Full Documentation

For complete logging documentation, see:

- **LOGGING_GUIDE.md** - Comprehensive logging guide
- **RUNBOOK.md** - Quick operational reference
- **TESTING_PLAN.md** - Test scenarios

---

## ✨ Enhanced Features

### New Logging Added:

- ✅ User ID tracking in all operations
- ✅ Command execution logging
- ✅ Service selection tracking
- ✅ Database operation counts
- ✅ Callback query details
- ✅ Error context with update IDs
- ✅ Timestamps on all log entries

### Log Format:

```
[Timestamp] [Level] [Component] Message with {Parameters}
```

Example:

```
2025-11-30 10:15:23 info: MassageBookingBot.BotWorker.Services.BotUpdateHandler[0]
      Processing message from user 123456789: /start
```

---

_Quick Reference - Keep this handy for rapid troubleshooting!_
