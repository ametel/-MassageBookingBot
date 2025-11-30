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
        var reminder24hTime = now.AddHours(24);
        var reminder2hTime = now.AddHours(2);

        // Get bookings needing 24h reminder
        var bookingsFor24h = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Service)
            .Where(b => !b.Reminder24hSent 
                && b.Status == Domain.Enums.BookingStatus.Confirmed
                && b.BookingDateTime > now
                && b.BookingDateTime <= reminder24hTime)
            .ToListAsync();

        foreach (var booking in bookingsFor24h)
        {
            var message = $"⏰ Reminder: Your massage appointment is in 24 hours!\n\n" +
                         $"Service: {booking.Service.Name}\n" +
                         $"Date: {booking.BookingDateTime:yyyy-MM-dd HH:mm}\n" +
                         $"Duration: {booking.Service.DurationMinutes} min";

            await _notificationService.SendReminderAsync(booking.User.TelegramUserId, message);
            booking.Reminder24hSent = true;
        }

        // Get bookings needing 2h reminder
        var bookingsFor2h = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Service)
            .Where(b => !b.Reminder2hSent 
                && b.Status == Domain.Enums.BookingStatus.Confirmed
                && b.BookingDateTime > now
                && b.BookingDateTime <= reminder2hTime)
            .ToListAsync();

        foreach (var booking in bookingsFor2h)
        {
            var message = $"⏰ Reminder: Your massage appointment is in 2 hours!\n\n" +
                         $"Service: {booking.Service.Name}\n" +
                         $"Date: {booking.BookingDateTime:yyyy-MM-dd HH:mm}\n" +
                         $"Duration: {booking.Service.DurationMinutes} min";

            await _notificationService.SendReminderAsync(booking.User.TelegramUserId, message);
            booking.Reminder2hSent = true;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"BookingReminderJob completed. Sent {bookingsFor24h.Count} 24h reminders and {bookingsFor2h.Count} 2h reminders");
    }
}
