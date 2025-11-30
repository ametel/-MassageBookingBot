# 📊 Logging Guide - Massage Booking Bot System

Complete guide to logging, monitoring, and troubleshooting the Massage Booking Bot application.

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Log Locations](#log-locations)
3. [Log Levels](#log-levels)
4. [Configuration](#configuration)
5. [Reading Logs](#reading-logs)
6. [Common Log Patterns](#common-log-patterns)
7. [Troubleshooting with Logs](#troubleshooting-with-logs)
8. [Log Analysis Tools](#log-analysis-tools)
9. [Production Recommendations](#production-recommendations)

---

## 🎯 Overview

The Massage Booking Bot system uses **ASP.NET Core Logging** with structured logging capabilities. Logs are written to:

- **Console** (real-time in terminal)
- **File** (persistent storage for analysis)

### What Gets Logged

#### API Server Logs:

- HTTP requests and responses
- Database operations
- API endpoint calls
- Authentication events
- Scheduled job execution (Quartz.NET)
- Errors and exceptions

#### Bot Worker Logs:

- Telegram bot updates received
- User commands and interactions
- Message processing
- Database operations
- Bot responses sent
- Errors and exceptions

---

## 📂 Log Locations

### Console Output

**Real-time logs** appear in the terminal where you run the services.

**API Server Terminal**:

```
cd K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api
dotnet run
```

Shows: API requests, database queries, scheduler events

**Bot Worker Terminal**:

```
cd K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.BotWorker
dotnet run
```

Shows: Telegram updates, user interactions, bot responses

### File Logs

**Configuration** (enabled in appsettings.json):

```json
"File": {
  "Path": "logs/api-.log",
  "Append": true,
  "FileSizeLimitBytes": 10485760,
  "MaxRollingFiles": 7
}
```

**Planned Locations** (when file logging is fully implemented):

| Component  | Log File Location                                             | Purpose                 |
| ---------- | ------------------------------------------------------------- | ----------------------- |
| API        | `src/MassageBookingBot.Api/logs/api-YYYYMMDD.log`             | API requests, responses |
| Bot Worker | `src/MassageBookingBot.BotWorker/logs/botworker-YYYYMMDD.log` | Bot interactions        |

**Current State**: Console logging is active. File logging requires additional NuGet package (Serilog recommended).

---

## 📊 Log Levels

Logs are categorized by severity:

| Level           | Description           | When to Use                  | Example                          |
| --------------- | --------------------- | ---------------------------- | -------------------------------- |
| **Trace**       | Very detailed         | Deep debugging               | Method entry/exit                |
| **Debug**       | Diagnostic info       | Development debugging        | Variable values, state           |
| **Information** | General info          | Normal operations            | User registered, booking created |
| **Warning**     | Unusual but not error | Potential issues             | No services available            |
| **Error**       | Operation failed      | Failures that need attention | Database connection failed       |
| **Critical**    | System-wide failure   | Severe issues                | Application crash                |

### Current Configuration

**API Server** (`appsettings.json`):

```json
"LogLevel": {
  "Default": "Information",
  "Microsoft.AspNetCore": "Warning",
  "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
  "MassageBookingBot": "Debug"
}
```

**Bot Worker** (`appsettings.json`):

```json
"LogLevel": {
  "Default": "Information",
  "Microsoft.Hosting.Lifetime": "Information",
  "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
  "MassageBookingBot": "Debug"
}
```

### What Each Level Captures

- **Default: Information** - All components log Info and above
- **Microsoft.AspNetCore: Warning** - Reduces ASP.NET Core noise
- **Microsoft.EntityFrameworkCore.Database.Command: Warning** - Hides SQL queries (change to Debug to see them)
- **MassageBookingBot: Debug** - Full debugging for our application code

---

## ⚙️ Configuration

### Change Log Levels

Edit the `appsettings.json` file for each service:

#### Make More Verbose (see more details):

```json
"LogLevel": {
  "Default": "Debug",
  "MassageBookingBot": "Debug",
  "Microsoft.EntityFrameworkCore.Database.Command": "Debug"
}
```

#### Make Less Verbose (production):

```json
"LogLevel": {
  "Default": "Information",
  "MassageBookingBot": "Information",
  "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
}
```

#### Environment-Specific Configuration

Create separate files for different environments:

- `appsettings.Development.json` - Development settings (already exists)
- `appsettings.Production.json` - Production settings
- `appsettings.Staging.json` - Staging settings

Example `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "MassageBookingBot": "Information"
    }
  }
}
```

### Enable File Logging (Recommended)

**Step 1**: Add Serilog NuGet packages

```powershell
cd src/MassageBookingBot.Api
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File

cd ../MassageBookingBot.BotWorker
dotnet add package Serilog.Extensions.Hosting
dotnet add package Serilog.Sinks.File
```

**Step 2**: Configure in `Program.cs` (API example):

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

---

## 📖 Reading Logs

### Console Log Format

**Standard Format**:

```
[Timestamp] [LogLevel] [Component] Message
```

**Example**:

```
2025-11-30 10:15:23 info: MassageBookingBot.BotWorker.Services.BotUpdateHandler[0]
      Processing message from user 123456789: /start
```

**Breaking Down a Log Entry**:

- `2025-11-30 10:15:23` - When the event occurred
- `info` - Log level (Information)
- `MassageBookingBot.BotWorker.Services.BotUpdateHandler` - Component that logged
- `[0]` - Event ID
- `Processing message from user 123456789: /start` - The message

### Structured Logging

The application uses **structured logging** with parameters:

```csharp
_logger.LogInformation("User {UserId} created booking {BookingId}", userId, bookingId);
```

**Console Output**:

```
info: User 123 created booking 456
```

**Benefit**: Parameters can be extracted and queried in log analysis tools.

---

## 🔍 Common Log Patterns

### Successful User Registration

```
info: MassageBookingBot.BotWorker.Services.BotUpdateHandler[0]
      Handling /start command for user 123456789 (@johndoe)
info: MassageBookingBot.BotWorker.Services.BotUpdateHandler[0]
      Creating new user 123456789 with username @johndoe
info: MassageBookingBot.BotWorker.Services.BotUpdateHandler[0]
      User 1 created successfully with referral code ABC123XY
info: MassageBookingBot.BotWorker.Services.BotUpdateHandler[0]
      Welcome message sent to user 123456789
```

### Booking Flow

```
info: Processing message from user 123456789: /book
info: Showing services to user 123456789
dbug: Found 5 active services
info: Service selection menu sent to user 123456789
info: Processing callback query from user 123456789: service_1
info: User 123456789 selected service 1
info: Showing available dates for service 1 to user 123456789
dbug: Generated 7 date options for next 7 days
info: Date selection menu sent to user 123456789
```

### API Request

```
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/api/services
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'MassageBookingBot.Api.Controllers.ServicesController.GetServices'
info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[3]
      Route matched with {action = "GetServices", controller = "Services"}
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/api/services - 200
```

### Scheduled Job Execution

```
info: Quartz.Core.QuartzScheduler[0]
      Scheduler QuartzScheduler_$_NON_CLUSTERED started.
info: Job BookingReminderJob is executing
info: Checking for bookings requiring reminders
info: Found 3 bookings requiring 24h reminder
info: Sent 24h reminder to user 123456789 for booking 1
```

### Error Scenarios

#### Database Error

```
fail: Microsoft.EntityFrameworkCore.Database.Command[20102]
      Failed executing DbCommand (6ms)
      SQLite Error 1: 'no such table: Users'
fail: MassageBookingBot.BotWorker.Services.BotUpdateHandler[0]
      Error handling update 12345 of type Message
      Microsoft.Data.Sqlite.SqliteException (0x80004005): SQLite Error 1: 'no such table: Users'
```

#### Telegram API Error

```
fail: MassageBookingBot.BotWorker.Worker[0]
      Error in Telegram Bot
      Telegram Bot API error 409: Conflict: terminated by other getUpdates request
```

#### Missing Configuration

```
fail: System.InvalidOperationException: Telegram Bot Token not configured
```

---

## 🛠️ Troubleshooting with Logs

### Problem: Bot Not Responding

**Look For**:

```
fail: Error in Telegram Bot
```

**Common Causes**:

1. Invalid bot token
2. Multiple bot instances running
3. Network issues

**Check**:

```powershell
# In terminal where Bot Worker is running, look for:
info: Bot @YourBotName is running

# If you see errors like:
fail: Telegram Bot API error 409
# → Multiple instances running
```

**Solution**:

```powershell
# Stop all dotnet processes
Get-Process -Name dotnet | Stop-Process -Force

# Restart bot worker
cd src\MassageBookingBot.BotWorker
dotnet run
```

### Problem: Database Errors

**Look For**:

```
fail: Microsoft.EntityFrameworkCore
SQLite Error
```

**Common Messages**:

- `no such table: Users` - Database not initialized
- `database is locked` - Connection issue
- `constraint failed` - Data validation error

**Check Database Location**:

```powershell
# API uses:
src\MassageBookingBot.Api\massagebooking.db

# Bot Worker should reference:
Data Source=..\MassageBookingBot.Api\massagebooking.db
```

**Solution**:

```powershell
# Delete and recreate database
Remove-Item src\MassageBookingBot.Api\massagebooking.db
# Restart API to recreate
```

### Problem: API Not Responding

**Look For**:

```
info: Now listening on: http://localhost:5000
```

**If Missing**:

- Port already in use
- Configuration error

**Check Port**:

```powershell
netstat -ano | findstr :5000
```

**Solution**:

```powershell
# Find and stop process using port 5000
Get-Process -Id <PID> | Stop-Process
```

### Problem: Missing Logs

**Look For**:

```
# No output in console
```

**Check Configuration**:

1. Verify `appsettings.json` has Logging section
2. Ensure `LogLevel` is not set to "None"
3. Check if console output is redirected

**Solution**:

```json
"LogLevel": {
  "Default": "Information"
}
```

### Problem: Too Many Logs

**Look For**:

```
# Excessive SQL queries
Executed DbCommand...
```

**Solution** - Reduce verbosity:

```json
"LogLevel": {
  "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
}
```

---

## 🔧 Log Analysis Tools

### PowerShell Log Analysis

#### View Last 50 Lines

```powershell
# If using file logging
Get-Content logs\api-20251130.log -Tail 50
```

#### Search for Errors

```powershell
Get-Content logs\api-20251130.log | Select-String -Pattern "fail:|Error"
```

#### Filter by User

```powershell
Get-Content logs\botworker-20251130.log | Select-String -Pattern "user 123456789"
```

#### Count Log Levels

```powershell
Get-Content logs\api-20251130.log | Group-Object {$_ -match "info:|warn:|fail:"} | Select-Object Count, Name
```

#### Real-Time Monitoring

```powershell
Get-Content logs\api-20251130.log -Wait -Tail 20
```

### Windows Event Viewer

For production, consider logging to Windows Event Log:

```csharp
// In Program.cs
builder.Logging.AddEventLog();
```

Then view in Event Viewer:

1. Open Event Viewer (eventvwr.msc)
2. Navigate to: Windows Logs → Application
3. Filter by Source: `.NET Runtime` or your app name

### Third-Party Tools

#### Seq (Recommended)

Free for development: https://datalust.co/seq

**Setup**:

```powershell
# Add Serilog.Sinks.Seq
dotnet add package Serilog.Sinks.Seq

# Configure in Program.cs
.WriteTo.Seq("http://localhost:5341")
```

**Benefits**:

- Web-based UI
- Structured log querying
- Charts and dashboards
- Real-time filtering

#### Other Options:

- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Splunk** (Enterprise)
- **Application Insights** (Azure)
- **Datadog** (Cloud-based)

---

## 🚀 Production Recommendations

### 1. Use File Logging

**Implement Serilog** for persistent logs:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10_485_760) // 10 MB
    .CreateLogger();
```

### 2. Adjust Log Levels

**Production appsettings.json**:

```json
"LogLevel": {
  "Default": "Warning",
  "MassageBookingBot": "Information",
  "Microsoft.AspNetCore": "Warning",
  "Microsoft.EntityFrameworkCore": "Warning"
}
```

**Benefits**:

- Reduces log volume
- Focuses on important events
- Improves performance

### 3. Enable Structured Logging

Always use **structured parameters**:

✅ **Good**:

```csharp
_logger.LogInformation("User {UserId} created booking {BookingId}", userId, bookingId);
```

❌ **Avoid**:

```csharp
_logger.LogInformation($"User {userId} created booking {bookingId}");
```

### 4. Implement Log Rotation

**Automatic rotation** by date:

```json
"File": {
  "RollingInterval": "Day",
  "RetainedFileCountLimit": 30
}
```

**Manual cleanup**:

```powershell
# Delete logs older than 30 days
Get-ChildItem logs -Filter "*.log" | Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-30)} | Remove-Item
```

### 5. Monitor Critical Errors

**Set up alerts** for critical issues:

- Database connection failures
- API errors (500s)
- Telegram bot disconnections
- Unhandled exceptions

### 6. Performance Logging

**Track performance metrics**:

```csharp
using var scope = _logger.BeginScope("BookingCreation");
var sw = Stopwatch.StartNew();

// ... create booking ...

_logger.LogInformation("Booking created in {ElapsedMs}ms", sw.ElapsedMilliseconds);
```

### 7. Security Considerations

**Never log**:

- Passwords
- API keys / tokens
- Personal data (GDPR)
- Credit card numbers

**Sanitize logs**:

```csharp
_logger.LogInformation("User {UserId} logged in", userId); // OK
// Don't log: username, email, phone unless necessary
```

---

## 📝 Quick Reference

### Essential PowerShell Commands

```powershell
# View real-time logs (console)
# Just watch the terminal where services are running

# Restart services cleanly
Get-Process -Name dotnet | Stop-Process -Force
cd src\MassageBookingBot.Api; dotnet run &
cd src\MassageBookingBot.BotWorker; dotnet run

# Search for errors in console history
# Copy-paste console output to file, then:
Select-String -Path console.txt -Pattern "fail:|error"

# Check if services are running
Get-Process -Name dotnet

# View listening ports
netstat -ano | findstr :5000
```

### Common Log Patterns to Watch

| Pattern                                    | Meaning                | Action                       |
| ------------------------------------------ | ---------------------- | ---------------------------- |
| `fail:`                                    | Error occurred         | Investigate immediately      |
| `warn:`                                    | Warning, not critical  | Review during analysis       |
| `SQLite Error`                             | Database problem       | Check DB file and connection |
| `Conflict: terminated by other getUpdates` | Duplicate bot instance | Stop all, restart one        |
| `No such table`                            | DB not initialized     | Restart API to create DB     |
| `401 Unauthorized`                         | Auth failed            | Check JWT configuration      |
| `Cannot access a disposed object`          | Resource cleanup issue | Restart service              |

### Log Level Quick Switch

**Development** (verbose):

```json
"Default": "Debug"
```

**Production** (minimal):

```json
"Default": "Warning"
```

**Troubleshooting** (maximum detail):

```json
"Default": "Trace",
"Microsoft.EntityFrameworkCore.Database.Command": "Debug"
```

---

## 🎯 Checklist for Effective Logging

- [x] Console logging configured
- [ ] File logging implemented (Serilog recommended)
- [ ] Log rotation configured
- [ ] Production log levels set appropriately
- [ ] Structured logging used consistently
- [ ] Sensitive data excluded from logs
- [ ] Error alerting configured
- [ ] Log retention policy defined
- [ ] Monitoring solution in place
- [ ] Team trained on log analysis

---

## 📚 Additional Resources

### Documentation

- [ASP.NET Core Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [Serilog Documentation](https://serilog.net/)
- [Structured Logging Best Practices](https://nblumhardt.com/2016/06/structured-logging-concepts/)

### Related Files

- `appsettings.json` - Log configuration
- `Program.cs` - Logging setup
- `BotUpdateHandler.cs` - Bot logging implementation

---

## 💡 Tips & Best Practices

1. **Log method entry/exit** in complex operations
2. **Include context** (userId, bookingId) in logs
3. **Use correlation IDs** to track requests across services
4. **Log exceptions** with full stack traces
5. **Avoid logging in loops** (use summary instead)
6. **Review logs regularly** to spot patterns
7. **Test logging** in development before production
8. **Keep logs secure** - restrict file permissions
9. **Backup important logs** for compliance
10. **Document custom log patterns** for your team

---

_Last Updated: November 30, 2025_
_For questions about logging, check the logs! 😄_
