using MassageBookingBot.Application.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MassageBookingBot.Infrastructure.Services;

public class GoogleCalendarService : ICalendarService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleCalendarService> _logger;
    private readonly Lazy<CalendarService> _calendarServiceLazy;
    private readonly string _calendarId;
    private bool _disposed;

    public GoogleCalendarService(IConfiguration configuration, ILogger<GoogleCalendarService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _calendarId = _configuration["GoogleCalendar:CalendarId"] ?? "primary";
        _calendarServiceLazy = new Lazy<CalendarService>(InitializeCalendarService);
    }

    private CalendarService CalendarService => _calendarServiceLazy.Value;

    private CalendarService InitializeCalendarService()
    {
        var credentialsPath = _configuration["GoogleCalendar:ServiceAccountKeyPath"];

        if (string.IsNullOrEmpty(credentialsPath))
        {
            throw new InvalidOperationException("GoogleCalendar:ServiceAccountKeyPath is not configured");
        }

        // Resolve to absolute path to handle relative paths correctly
        var absolutePath = Path.IsPathRooted(credentialsPath) 
            ? credentialsPath 
            : Path.GetFullPath(credentialsPath);

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException($"Service account key file not found at: {absolutePath}");
        }

        try
        {
            // Load service account credentials
            GoogleCredential credential;
            using (var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(CalendarService.Scope.Calendar);
            }

            // Initialize Calendar service
            var calendarService = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _configuration["GoogleCalendar:ApplicationName"] ?? "Massage Booking Bot"
            });

            _logger.LogInformation("Google Calendar service initialized successfully");
            return calendarService;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Google Calendar service");
            throw;
        }
    }

    public async Task<string> CreateEventAsync(string title, string description, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time", nameof(endTime));

        try
        {
            var calendarEvent = new Event
            {
                Summary = title,
                Description = description,
                Start = new EventDateTime
                {
                    DateTime = startTime,
                    TimeZone = _configuration["GoogleCalendar:TimeZone"] ?? "UTC"
                },
                End = new EventDateTime
                {
                    DateTime = endTime,
                    TimeZone = _configuration["GoogleCalendar:TimeZone"] ?? "UTC"
                },
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new[]
                    {
                        new EventReminder { Method = "email", Minutes = 24 * 60 },
                        new EventReminder { Method = "popup", Minutes = 120 }
                    }
                }
            };

            var request = CalendarService.Events.Insert(calendarEvent, _calendarId);
            var createdEvent = await request.ExecuteAsync(cancellationToken);

            _logger.LogInformation("Created calendar event {EventId} for {Title} at {StartTime}", 
                createdEvent.Id, title, startTime);

            return createdEvent.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create calendar event: {Title} at {StartTime}", title, startTime);
            throw;
        }
    }

    public async Task UpdateEventAsync(string eventId, string title, string description, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            _logger.LogWarning("Attempted to update event with null or empty eventId");
            throw new ArgumentException("Event ID cannot be null or empty", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time", nameof(endTime));

        try
        {
            // Get existing event
            var existingEvent = await CalendarService.Events.Get(_calendarId, eventId).ExecuteAsync(cancellationToken);

            // Update event properties
            existingEvent.Summary = title;
            existingEvent.Description = description;
            existingEvent.Start = new EventDateTime
            {
                DateTime = startTime,
                TimeZone = _configuration["GoogleCalendar:TimeZone"] ?? "UTC"
            };
            existingEvent.End = new EventDateTime
            {
                DateTime = endTime,
                TimeZone = _configuration["GoogleCalendar:TimeZone"] ?? "UTC"
            };

            var request = CalendarService.Events.Update(existingEvent, _calendarId, eventId);
            await request.ExecuteAsync(cancellationToken);

            _logger.LogInformation("Updated calendar event {EventId} for {Title} at {StartTime}", 
                eventId, title, startTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update calendar event {EventId}", eventId);
            throw;
        }
    }

    public async Task DeleteEventAsync(string eventId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            _logger.LogWarning("Attempted to delete event with null or empty eventId");
            throw new ArgumentException("Event ID cannot be null or empty", nameof(eventId));
        }

        try
        {
            var request = CalendarService.Events.Delete(_calendarId, eventId);
            await request.ExecuteAsync(cancellationToken);

            _logger.LogInformation("Deleted calendar event {EventId}", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete calendar event {EventId}", eventId);
            throw;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_calendarServiceLazy.IsValueCreated)
            {
                _calendarServiceLazy.Value?.Dispose();
            }
            _disposed = true;
        }
    }
}
