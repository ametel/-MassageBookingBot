using MassageBookingBot.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace MassageBookingBot.Infrastructure.Jobs;

public class BookingReminderJob : IJob
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BookingReminderJob> _logger;

    public BookingReminderJob(
        IApplicationDbContext context,
        INotificationService notificationService,
        ILogger<BookingReminderJob> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("BookingReminderJob started");

        var now = DateTime.UtcNow;
        var reminder24hWindowStart = now.AddHours(23);
        var reminder24hWindowEnd = now.AddHours(25);
        var reminder2hWindowStart = now.AddHours(1.5);
        var reminder2hWindowEnd = now.AddHours(2.5);

        // Get bookings needing 24h reminder (window: 23-25 hours from now)
        var bookingsFor24h = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Service)
            .Where(b => !b.Reminder24hSent 
                && b.Status == Domain.Enums.BookingStatus.Confirmed
                && b.BookingDateTime >= reminder24hWindowStart
                && b.BookingDateTime <= reminder24hWindowEnd)
            .ToListAsync();

        _logger.LogInformation("Found {Count} bookings needing 24h reminder", bookingsFor24h.Count);

        foreach (var booking in bookingsFor24h)
        {
            try
            {
                var message = $"⏰ Reminder: Your massage appointment is in 24 hours!\n\n" +
                             $"Service: {booking.Service.Name}\n" +
                             $"Date: {booking.BookingDateTime:yyyy-MM-dd HH:mm}\n" +
                             $"Duration: {booking.Service.DurationMinutes} min";

                await _notificationService.SendReminderAsync(booking.User.TelegramUserId, message);
                booking.Reminder24hSent = true;
                _logger.LogInformation("Sent 24h reminder for booking {BookingId} to user {UserId}", 
                    booking.Id, booking.User.TelegramUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send 24h reminder for booking {BookingId}", booking.Id);
            }
        }

        // Get bookings needing 2h reminder (window: 1.5-2.5 hours from now)
        var bookingsFor2h = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Service)
            .Where(b => !b.Reminder2hSent 
                && b.Status == Domain.Enums.BookingStatus.Confirmed
                && b.BookingDateTime >= reminder2hWindowStart
                && b.BookingDateTime <= reminder2hWindowEnd)
            .ToListAsync();

        _logger.LogInformation("Found {Count} bookings needing 2h reminder", bookingsFor2h.Count);

        foreach (var booking in bookingsFor2h)
        {
            try
            {
                var message = $"⏰ Reminder: Your massage appointment is in 2 hours!\n\n" +
                             $"Service: {booking.Service.Name}\n" +
                             $"Date: {booking.BookingDateTime:yyyy-MM-dd HH:mm}\n" +
                             $"Duration: {booking.Service.DurationMinutes} min";

                await _notificationService.SendReminderAsync(booking.User.TelegramUserId, message);
                booking.Reminder2hSent = true;
                _logger.LogInformation("Sent 2h reminder for booking {BookingId} to user {UserId}", 
                    booking.Id, booking.User.TelegramUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send 2h reminder for booking {BookingId}", booking.Id);
            }
        }

        if (bookingsFor24h.Any() || bookingsFor2h.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("BookingReminderJob completed. Sent {Count24h} 24h reminders and {Count2h} 2h reminders", 
                bookingsFor24h.Count, bookingsFor2h.Count);
        }
        else
        {
            _logger.LogInformation("BookingReminderJob completed. No reminders to send");
        }
    }
}
