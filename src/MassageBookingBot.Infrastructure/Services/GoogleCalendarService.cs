using MassageBookingBot.Application.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MassageBookingBot.Infrastructure.Services;

public class GoogleCalendarService : ICalendarService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleCalendarService> _logger;
    private readonly CalendarService _calendarService;
    private readonly string _calendarId;

    public GoogleCalendarService(IConfiguration configuration, ILogger<GoogleCalendarService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var credentialsPath = _configuration["GoogleCalendar:ServiceAccountKeyPath"];
        _calendarId = _configuration["GoogleCalendar:CalendarId"] ?? "primary";

        if (string.IsNullOrEmpty(credentialsPath))
        {
            throw new InvalidOperationException("GoogleCalendar:ServiceAccountKeyPath is not configured");
        }

        if (!File.Exists(credentialsPath))
        {
            throw new FileNotFoundException($"Service account key file not found at: {credentialsPath}");
        }

        try
        {
            // Load service account credentials
            GoogleCredential credential;
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(CalendarService.Scope.Calendar);
            }

            // Initialize Calendar service
            _calendarService = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _configuration["GoogleCalendar:ApplicationName"] ?? "Massage Booking Bot"
            });

            _logger.LogInformation("Google Calendar service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Google Calendar service");
            throw;
        }
    }

    public async Task<string> CreateEventAsync(string title, string description, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
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

            var request = _calendarService.Events.Insert(calendarEvent, _calendarId);
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
        try
        {
            // Get existing event
            var existingEvent = await _calendarService.Events.Get(_calendarId, eventId).ExecuteAsync(cancellationToken);

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

            var request = _calendarService.Events.Update(existingEvent, _calendarId, eventId);
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
        try
        {
            var request = _calendarService.Events.Delete(_calendarId, eventId);
            await request.ExecuteAsync(cancellationToken);

            _logger.LogInformation("Deleted calendar event {EventId}", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete calendar event {EventId}", eventId);
            throw;
        }
    }
}
