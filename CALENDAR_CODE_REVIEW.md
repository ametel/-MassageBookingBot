# Google Calendar Integration - Senior Developer Code Review

## Executive Summary

**Overall Assessment**: 🟡 **GOOD** with Critical Issues to Address

The implementation demonstrates solid understanding of Google Calendar API integration with service accounts. However, there are **several critical production issues** that must be addressed before deployment.

---

## 🔴 Critical Issues (Must Fix)

### 1. **Resource Leak - CalendarService Not Disposed**

**Severity**: CRITICAL  
**Location**: `GoogleCalendarService.cs`

**Issue**:

```csharp
private readonly CalendarService _calendarService;
```

`CalendarService` implements `IDisposable` but is never disposed. This is created in the constructor and held for the lifetime of the service, causing resource leaks.

**Impact**:

- HTTP client connections not properly released
- Memory leaks over time
- Connection pool exhaustion under load
- Application instability in production

**Solution**:

```csharp
public class GoogleCalendarService : ICalendarService, IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        if (!_disposed)
        {
            _calendarService?.Dispose();
            _disposed = true;
        }
    }
}
```

**Alternative Better Solution**:
Don't create `CalendarService` in constructor. Create it on-demand or use factory pattern with proper disposal.

---

### 2. **Scoped Service with Singleton Behavior**

**Severity**: CRITICAL  
**Location**: `DependencyInjection.cs`

**Issue**:

```csharp
services.AddScoped<ICalendarService, GoogleCalendarService>();
```

Service is registered as Scoped, but creates expensive resources (file I/O, credentials, HTTP client) in constructor **on every request**.

**Impact**:

- Performance degradation (re-reading file, re-authenticating per request)
- Unnecessary file I/O operations
- Slower response times
- Increased resource consumption

**Solution**:
Register as Singleton and implement thread-safe operations:

```csharp
services.AddSingleton<ICalendarService, GoogleCalendarService>();
```

All operations are already async and thread-safe via Google API client.

---

### 3. **Constructor Doing Heavy Work**

**Severity**: HIGH  
**Location**: `GoogleCalendarService.cs` constructor

**Issue**:

```csharp
public GoogleCalendarService(IConfiguration configuration, ILogger<GoogleCalendarService> logger)
{
    // File I/O in constructor
    using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
    {
        credential = GoogleCredential.FromStream(stream)
            .CreateScoped(CalendarService.Scope.Calendar);
    }
    // ...
}
```

**Problems**:

- Constructor throws exceptions (violates best practices)
- File I/O during DI resolution
- Cannot be tested easily
- Makes application startup fail if calendar misconfigured

**Impact**:

- Application won't start if Google Calendar unavailable
- Difficult to unit test
- Violates SOLID principles

**Solution**:
Use lazy initialization or factory pattern:

```csharp
private CalendarService? _calendarService;
private readonly Lazy<CalendarService> _lazyCalendarService;

public GoogleCalendarService(...)
{
    _lazyCalendarService = new Lazy<CalendarService>(InitializeCalendarService);
}

private CalendarService CalendarService => _lazyCalendarService.Value;
```

---

### 4. **Console.WriteLine Instead of Logger**

**Severity**: MEDIUM  
**Location**: All command handlers

**Issue**:

```csharp
catch (Exception ex)
{
    Console.WriteLine($"Failed to create calendar event: {ex.Message}");
}
```

**Problems**:

- Doesn't integrate with logging infrastructure
- No log levels, no context
- Can't be monitored or alerted
- Lost in production environments

**Impact**:

- Silently failing operations
- No visibility into production issues
- Difficult to debug

**Solution**:
Inject `ILogger` into command handlers:

```csharp
private readonly ILogger<CreateBookingCommandHandler> _logger;

catch (Exception ex)
{
    _logger.LogError(ex, "Failed to create calendar event for booking {BookingId}", booking.Id);
}
```

---

### 5. **No Null Check on Event ID in UpdateEventAsync**

**Severity**: MEDIUM  
**Location**: `GoogleCalendarService.UpdateEventAsync()`

**Issue**:

```csharp
public async Task UpdateEventAsync(string eventId, ...)
{
    var existingEvent = await _calendarService.Events.Get(_calendarId, eventId)
        .ExecuteAsync(cancellationToken);
}
```

No validation that `eventId` is not null or empty before calling Google API.

**Impact**:

- API exception if eventId is null/empty
- Poor error messages
- Unnecessary API calls

**Solution**:

```csharp
if (string.IsNullOrWhiteSpace(eventId))
{
    _logger.LogWarning("Attempted to update event with null or empty eventId");
    return; // or throw ArgumentException
}
```

---

### 6. **CancelBookingCommand Doesn't Handle Calendar Failure**

**Severity**: HIGH  
**Location**: `CancelBookingCommand.cs`

**Issue**:

```csharp
if (!string.IsNullOrEmpty(booking.GoogleCalendarEventId))
{
    await _calendarService.DeleteEventAsync(booking.GoogleCalendarEventId, cancellationToken);
}
```

No try-catch. If calendar deletion fails, entire cancellation fails and transaction rolls back.

**Impact**:

- User can't cancel bookings if Google Calendar is down
- Booking stuck in confirmed state
- Poor user experience

**Solution**:

```csharp
if (!string.IsNullOrEmpty(booking.GoogleCalendarEventId))
{
    try
    {
        await _calendarService.DeleteEventAsync(booking.GoogleCalendarEventId, cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to delete calendar event {EventId} for booking {BookingId}",
            booking.GoogleCalendarEventId, booking.Id);
        // Continue with cancellation even if calendar deletion fails
    }
}
```

---

### 7. **UpdateBookingCommand Missing Transaction**

**Severity**: MEDIUM  
**Location**: `UpdateBookingCommand.cs`

**Issue**:
No transaction wrapper, but multiple database operations and external API call.

**Impact**:

- Partial updates possible
- Data inconsistency if calendar update succeeds but save fails
- No atomicity guarantee

**Solution**:
Wrap in transaction like CreateBookingCommand:

```csharp
await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
try
{
    // ... all operations
    await _context.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);
}
catch
{
    await transaction.RollbackAsync(cancellationToken);
    throw;
}
```

---

## 🟡 Medium Priority Issues

### 8. **Hard-Coded Reminder Times**

**Location**: `GoogleCalendarService.CreateEventAsync()`

```csharp
new EventReminder { Method = "email", Minutes = 24 * 60 },
new EventReminder { Method = "popup", Minutes = 120 }
```

Should be configurable in appsettings.json.

---

### 9. **No Retry Logic for Transient Failures**

Google Calendar API can have transient network failures. No retry policy implemented.

**Recommendation**: Use Polly library for retry with exponential backoff.

---

### 10. **Path Resolution Issues**

```csharp
var credentialsPath = _configuration["GoogleCalendar:ServiceAccountKeyPath"];
```

Relative paths may not work correctly depending on working directory.

**Solution**: Always resolve to absolute path:

```csharp
var credentialsPath = Path.GetFullPath(_configuration["GoogleCalendar:ServiceAccountKeyPath"]!);
```

---

### 11. **No Health Check for Calendar Service**

Service might be misconfigured but application starts. No way to monitor if calendar integration is healthy.

**Recommendation**: Implement `IHealthCheck` that verifies:

- Credentials file exists
- Can authenticate
- Can access calendar
- API quota not exceeded

---

### 12. **UpdateBookingCommand Inefficient Service Reload**

```csharp
booking.Service = await _context.Services.FindAsync([request.Booking.ServiceId.Value], cancellationToken)
    ?? booking.Service;
```

Only needed if service changed, but Include already loaded it.

---

### 13. **No Idempotency**

If CreateBooking is called twice, creates two calendar events. Should check for existing events or use idempotency keys.

---

## 🟢 Good Practices Observed

✅ **Proper Interface Abstraction** - `ICalendarService` allows easy testing and swapping implementations  
✅ **Async/Await Pattern** - Correctly implemented throughout  
✅ **CancellationToken Support** - Proper cancellation handling  
✅ **Logging in Service Layer** - Good structured logging in GoogleCalendarService  
✅ **Configuration Externalization** - Settings in appsettings.json  
✅ **Service Account Authentication** - Correct approach for server-to-server  
✅ **Transaction Usage** - CreateBookingCommand properly uses transactions  
✅ **Non-Blocking Calendar Failures** - Good resilience pattern in CreateBookingCommand

---

## 🔧 Recommended Refactoring

### Option 1: Lazy Singleton with Proper Disposal

```csharp
public class GoogleCalendarService : ICalendarService, IAsyncDisposable
{
    private readonly Lazy<CalendarService> _calendarServiceLazy;

    public GoogleCalendarService(IConfiguration configuration, ILogger<GoogleCalendarService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _calendarServiceLazy = new Lazy<CalendarService>(InitializeCalendarService);
    }

    private CalendarService InitializeCalendarService()
    {
        // All initialization logic here
        // Can throw exceptions safely after app startup
    }

    private CalendarService CalendarService => _calendarServiceLazy.Value;

    public async ValueTask DisposeAsync()
    {
        if (_calendarServiceLazy.IsValueCreated)
        {
            _calendarServiceLazy.Value?.Dispose();
        }
    }
}
```

Register as:

```csharp
services.AddSingleton<ICalendarService, GoogleCalendarService>();
```

---

### Option 2: Factory Pattern

```csharp
public interface ICalendarServiceFactory
{
    ICalendarService Create();
}

public class GoogleCalendarServiceFactory : ICalendarServiceFactory
{
    // Create service with proper lifecycle management
}

services.AddSingleton<ICalendarServiceFactory, GoogleCalendarServiceFactory>();
services.AddScoped<ICalendarService>(sp => sp.GetRequiredService<ICalendarServiceFactory>().Create());
```

---

## 📊 Performance Considerations

### Current Issues:

1. **File I/O per request** - Reading credentials file every time
2. **Authentication overhead** - Creating credentials per request
3. **No connection pooling optimization**

### Expected Impact:

- **Latency**: ~100-200ms additional per booking operation
- **Throughput**: Limited by sequential file reads
- **Scalability**: Poor under high concurrent load

### After Fixes:

- **Latency**: ~20-50ms for calendar operations
- **Throughput**: 100+ operations/second
- **Scalability**: Linear with proper singleton pattern

---

## 🧪 Testing Gaps

### Missing Tests:

1. Unit tests for GoogleCalendarService (need to mock CalendarService)
2. Integration tests with actual Google Calendar
3. Error scenario tests (network failures, invalid credentials)
4. Load tests for concurrent operations
5. Test for resource disposal

### Recommended Test Structure:

```csharp
public class GoogleCalendarServiceTests
{
    [Fact]
    public async Task CreateEventAsync_WithValidData_ReturnsEventId() { }

    [Fact]
    public async Task CreateEventAsync_WithInvalidCredentials_ThrowsException() { }

    [Fact]
    public async Task UpdateEventAsync_WithNullEventId_ThrowsArgumentException() { }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow() { }
}
```

---

## 🔒 Security Review

### ✅ Good:

- Service account approach (no user OAuth)
- Credentials not in source control
- Proper scope limiting (`Calendar` scope only)

### ⚠️ Concerns:

1. **No credential encryption** - JSON file stored in plaintext
2. **No access logging** - Can't audit who accessed calendar
3. **No rate limiting** - Could exhaust API quota
4. **File permissions not enforced** - Should be 600 or 400

### Recommendations:

1. Use Azure Key Vault / AWS Secrets Manager in production
2. Implement audit logging for all calendar operations
3. Add rate limiting middleware
4. Document secure deployment practices

---

## 📋 Production Readiness Checklist

- [ ] Fix resource disposal (CRITICAL)
- [ ] Change to Singleton registration (CRITICAL)
- [ ] Add try-catch in CancelBookingCommand (HIGH)
- [ ] Replace Console.WriteLine with ILogger (MEDIUM)
- [ ] Add input validation for eventId (MEDIUM)
- [ ] Add transaction to UpdateBookingCommand (MEDIUM)
- [ ] Implement health checks (MEDIUM)
- [ ] Add retry logic with Polly (MEDIUM)
- [ ] Make reminder times configurable (LOW)
- [ ] Add comprehensive unit tests (HIGH)
- [ ] Load test calendar operations (MEDIUM)
- [ ] Security: Use secret manager (HIGH)
- [ ] Security: Implement audit logging (MEDIUM)
- [ ] Documentation: Deployment guide (MEDIUM)

---

## 🎯 Priority Action Items

### Week 1 (MUST DO):

1. Implement IDisposable/IAsyncDisposable
2. Change to Singleton registration
3. Add error handling in CancelBookingCommand
4. Replace Console.WriteLine with ILogger throughout

### Week 2 (SHOULD DO):

1. Add input validation
2. Implement health checks
3. Add retry logic for transient failures
4. Write unit tests

### Week 3 (NICE TO HAVE):

1. Make configuration more flexible
2. Implement audit logging
3. Performance optimization
4. Load testing

---

## 💭 Final Recommendations

### For MVP/Testing:

Current implementation is **acceptable** with quick fixes:

- Add Dispose implementation
- Fix CancelBookingCommand error handling
- Replace Console.WriteLine

### For Production:

Must address **all critical issues** plus:

- Comprehensive testing
- Secret management
- Monitoring and alerting
- Performance testing under load

### Architecture Score: 7/10

- Good separation of concerns
- Proper interface abstractions
- Clean code structure
- Needs better resource management
- Needs better error handling

### Code Quality Score: 6/10

- Good async patterns
- Poor logging practices
- Missing disposal pattern
- Inconsistent error handling
- Good transaction usage (partially)

---

## 📖 Learning Resources

For the team:

1. [IDisposable Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose)
2. [DI Service Lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
3. [Polly Retry Policies](https://github.com/App-vNext/Polly)
4. [Google Calendar API Best Practices](https://developers.google.com/calendar/api/guides/best-practices)

---

## Conclusion

The implementation shows **good understanding of core concepts** but has **critical production issues** that must be addressed. With the recommended fixes, this will be a solid, production-ready integration.

**Estimated Effort to Production-Ready**: 2-3 days for a senior developer

**Risk Level**: 🟡 MEDIUM (after critical fixes) / 🔴 HIGH (as-is)
