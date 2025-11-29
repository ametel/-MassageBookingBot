namespace MassageBookingBot.Application.Interfaces;

public interface ICalendarService
{
    Task<string> CreateEventAsync(string title, string description, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
    Task UpdateEventAsync(string eventId, string title, string description, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(string eventId, CancellationToken cancellationToken = default);
}
