# Google Calendar Integration Guide

This guide explains how to integrate the Massage Booking Bot with Google Calendar using a service account.

## Overview

The bot uses Google Calendar API with service account authentication to automatically create, update, and delete calendar events when bookings are made, modified, or cancelled.

## Implementation

### CalendarServiceWrapper (GoogleCalendarService)

Located at: `src/MassageBookingBot.Infrastructure/Services/GoogleCalendarService.cs`

The service implements `ICalendarService` interface with three main methods:

#### 1. AddEvent (CreateEventAsync)
```csharp
Task<string> CreateEventAsync(string title, string description, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
```
- **Purpose**: Creates a new event in Google Calendar
- **Returns**: Event ID for future reference
- **Features**: 
  - Sets custom reminders (24 hours email, 2 hours popup)
  - Uses configured timezone
  - Stores event ID in database for later updates/deletions

#### 2. UpdateEvent (UpdateEventAsync)
```csharp
Task UpdateEventAsync(string eventId, string title, string description, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
```
- **Purpose**: Updates an existing calendar event
- **Use Case**: When booking time or service is changed
- **Implementation**: Fetches existing event and updates properties

#### 3. RemoveEvent (DeleteEventAsync)
```csharp
Task DeleteEventAsync(string eventId, CancellationToken cancellationToken = default)
```
- **Purpose**: Deletes a calendar event
- **Use Case**: When booking is cancelled

### Integration Points

#### CreateBookingCommand
**File**: `src/MassageBookingBot.Application/Commands/Bookings/CreateBookingCommand.cs`

When a new booking is created:
1. Booking is saved to database
2. Calendar event is created with service details
3. Event ID is stored in `Booking.GoogleCalendarEventId`
4. Confirmation is sent to user via Telegram

```csharp
var eventId = await _calendarService.CreateEventAsync(
    $"Massage: {service.Name}",
    $"Client: {user.FirstName} {user.LastName}\nPhone: {user.PhoneNumber}",
    booking.BookingDateTime,
    endTime,
    cancellationToken);

booking.GoogleCalendarEventId = eventId;
```

#### UpdateBookingCommand
**File**: `src/MassageBookingBot.Application/Commands/Bookings/UpdateBookingCommand.cs`

When a booking is updated:
1. Booking changes are saved
2. If time or service changed, calendar event is updated
3. User receives update notification

```csharp
await _calendarService.UpdateEventAsync(
    booking.GoogleCalendarEventId,
    $"Massage: {booking.Service.Name}",
    $"Client: {booking.User.FirstName} {booking.User.LastName}\nPhone: {booking.User.PhoneNumber}",
    booking.BookingDateTime,
    endTime,
    cancellationToken);
```

#### CancelBookingCommand
**File**: `src/MassageBookingBot.Application/Commands/Bookings/CancelBookingCommand.cs`

When a booking is cancelled:
1. Booking status is updated to Cancelled
2. Calendar event is deleted from Google Calendar

```csharp
if (!string.IsNullOrEmpty(booking.GoogleCalendarEventId))
{
    await _calendarService.DeleteEventAsync(booking.GoogleCalendarEventId, cancellationToken);
}
```

## Setup Instructions

### 1. Create Google Cloud Project & Service Account

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing one
3. Enable Google Calendar API:
   - Go to "APIs & Services" > "Library"
   - Search for "Google Calendar API"
   - Click "Enable"

4. Create Service Account:
   - Go to "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "Service Account"
   - Fill in details and click "Create"
   - Skip granting additional roles (click "Continue", then "Done")

5. Create Service Account Key:
   - Click on the created service account
   - Go to "Keys" tab
   - Click "Add Key" > "Create New Key"
   - Select "JSON" and click "Create"
   - Save the downloaded JSON file as `google-service-account.json`

### 2. Share Calendar with Service Account

1. Open Google Calendar
2. Find the calendar you want to use (or create a new one)
3. Click settings (three dots) next to calendar name
4. Select "Settings and sharing"
5. Scroll to "Share with specific people"
6. Click "Add people"
7. Enter the service account email (found in JSON file: `client_email`)
8. Set permission to "Make changes to events"
9. Click "Send"

### 3. Configure Application

1. Copy `google-service-account.json` to:
   - `src/MassageBookingBot.Api/` directory
   - Or specify custom path in appsettings.json

2. Update `appsettings.json` and `appsettings.Development.json`:

```json
{
  "GoogleCalendar": {
    "ServiceAccountKeyPath": "google-service-account.json",
    "CalendarId": "your-calendar-id@group.calendar.google.com",
    "TimeZone": "America/New_York",
    "ApplicationName": "Massage Booking Bot"
  }
}
```

**Getting Calendar ID:**
- In Google Calendar settings, go to calendar settings
- Scroll to "Integrate calendar"
- Copy the "Calendar ID" (looks like: `abc123@group.calendar.google.com`)
- Use `"primary"` for the service account's default calendar

**Available TimeZones:**
- Use IANA timezone names (e.g., "America/New_York", "Europe/London", "Asia/Tokyo")
- See full list: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones

### 4. Security Considerations

**DO NOT commit the service account JSON file to source control!**

Add to `.gitignore`:
```
google-service-account.json
**/google-service-account*.json
```

For production deployment:
- Store service account key in secure secret management (Azure Key Vault, AWS Secrets Manager, etc.)
- Use environment variables to pass the path
- Restrict file permissions (600 or 400)

## Configuration Reference

### appsettings.json Structure

```json
{
  "GoogleCalendar": {
    "ServiceAccountKeyPath": "google-service-account.json",
    "CalendarId": "primary",
    "TimeZone": "UTC",
    "ApplicationName": "Massage Booking Bot"
  }
}
```

**Parameters:**
- `ServiceAccountKeyPath` (required): Path to the service account JSON key file
- `CalendarId` (required): Google Calendar ID or "primary" for default
- `TimeZone` (optional): IANA timezone name, defaults to "UTC"
- `ApplicationName` (optional): Application name shown in API requests

## Testing

### Manual Testing

1. Create a booking via API:
```bash
POST /api/bookings
{
  "userId": 1,
  "serviceId": 1,
  "bookingDateTime": "2025-12-01T14:00:00",
  "notes": "Test booking"
}
```

2. Check Google Calendar for the created event

3. Update the booking:
```bash
PUT /api/bookings/1
{
  "bookingDateTime": "2025-12-01T15:00:00"
}
```

4. Verify event time changed in Google Calendar

5. Cancel the booking:
```bash
DELETE /api/bookings/1
```

6. Verify event is removed from Google Calendar

### Troubleshooting

**Error: "Service account key file not found"**
- Check the path in `ServiceAccountKeyPath`
- Ensure file exists and is accessible
- Use absolute path if relative path doesn't work

**Error: "Failed to initialize Google Calendar service"**
- Verify JSON file is valid service account key
- Check file permissions
- Ensure Google Calendar API is enabled in Cloud Console

**Error: "Failed to create calendar event"**
- Verify calendar is shared with service account email
- Check service account has "Make changes to events" permission
- Verify CalendarId is correct

**Error: "Not found" when updating/deleting**
- Event may have been manually deleted from calendar
- Event ID stored in database may be invalid
- Calendar may have been unshared from service account

## Error Handling

The implementation includes graceful error handling:

- Calendar operations are wrapped in try-catch blocks
- Failures are logged but don't prevent booking operations
- Bookings succeed even if calendar sync fails
- This ensures system availability even when Google Calendar is down

Example from CreateBookingCommand:
```csharp
try
{
    var eventId = await _calendarService.CreateEventAsync(/*...*/);
    booking.GoogleCalendarEventId = eventId;
}
catch (Exception ex)
{
    // Log but don't fail the booking
    Console.WriteLine($"Failed to create calendar event: {ex.Message}");
}
```

## Dependencies

### NuGet Packages

Added to `MassageBookingBot.Infrastructure.csproj`:

```xml
<PackageReference Include="Google.Apis.Calendar.v3" Version="1.72.0.3953" />
<PackageReference Include="Google.Apis.Auth" Version="1.72.0" />
```

These provide:
- Google Calendar API client
- Service account authentication
- OAuth2 credential management

## API Documentation

### Endpoints

#### Create Booking (with Calendar Event)
```
POST /api/bookings
Content-Type: application/json

{
  "userId": 1,
  "serviceId": 1,
  "bookingDateTime": "2025-12-01T14:00:00",
  "notes": "Optional notes"
}

Response: 201 Created
{
  "id": 1
}
```

#### Update Booking (with Calendar Sync)
```
PUT /api/bookings/{id}
Content-Type: application/json

{
  "serviceId": 2,
  "bookingDateTime": "2025-12-01T15:00:00",
  "notes": "Updated notes"
}

Response: 204 No Content
```

#### Cancel Booking (removes Calendar Event)
```
DELETE /api/bookings/{id}

Response: 204 No Content
```

## Architecture

```
┌─────────────────────────────────────────┐
│         Booking Commands                │
│  (CreateBooking, UpdateBooking, etc.)   │
└─────────────┬───────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│        ICalendarService                 │
│  (Interface in Application Layer)       │
└─────────────┬───────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│     GoogleCalendarService               │
│  (Implementation in Infrastructure)     │
│  - Service Account Authentication       │
│  - CRUD operations on calendar events   │
└─────────────┬───────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│      Google Calendar API                │
│   (External Service)                    │
└─────────────────────────────────────────┘
```

## Future Enhancements

Potential improvements:

1. **Batch Operations**: Update multiple events in single API call
2. **Calendar Selection**: Support multiple calendars per service type
3. **Advanced Reminders**: Configurable reminder times per service
4. **Attendee Management**: Add client email as attendee if available
5. **Recurrence**: Support recurring bookings (weekly massages)
6. **Availability Sync**: Query calendar to check slot availability
7. **Conflict Detection**: Prevent double-booking using calendar availability
8. **Calendar Colors**: Color-code events by service type or status

## Support

For issues or questions:
- Check logs in `logs/` directory
- Review Google Cloud Console logs
- Verify service account permissions in Google Calendar
- Ensure API quotas are not exceeded (Calendar API has generous free tier)
