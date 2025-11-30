using MassageBookingBot.Application.DTOs;
using MassageBookingBot.Application.Interfaces;
using MassageBookingBot.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MassageBookingBot.Application.Commands.Bookings;

public record UpdateBookingCommand(int BookingId, UpdateBookingDto Booking) : IRequest<bool>;

public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalendarService _calendarService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<UpdateBookingCommandHandler> _logger;

    public UpdateBookingCommandHandler(
        IApplicationDbContext context,
        ICalendarService calendarService,
        INotificationService notificationService,
        ILogger<UpdateBookingCommandHandler> logger)
    {
        _context = context;
        _calendarService = calendarService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Service)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking == null)
            return false;

        // Update booking properties
        var originalDateTime = booking.BookingDateTime;
        var originalServiceId = booking.ServiceId;
        
        if (request.Booking.BookingDateTime.HasValue)
            booking.BookingDateTime = request.Booking.BookingDateTime.Value;
        
        if (request.Booking.ServiceId.HasValue)
            booking.ServiceId = request.Booking.ServiceId.Value;
        
        if (request.Booking.Notes != null)
            booking.Notes = request.Booking.Notes;
        
        booking.UpdatedAt = DateTime.UtcNow;

        // Update Google Calendar event if datetime or service changed
        if ((request.Booking.BookingDateTime.HasValue || request.Booking.ServiceId.HasValue) 
            && !string.IsNullOrEmpty(booking.GoogleCalendarEventId))
        {
            try
            {
                // Reload service if changed
                if (request.Booking.ServiceId.HasValue && originalServiceId != request.Booking.ServiceId.Value)
                {
                    booking.Service = await _context.Services.FindAsync([request.Booking.ServiceId.Value], cancellationToken) 
                        ?? booking.Service;
                }

                var endTime = booking.BookingDateTime.AddMinutes(booking.Service.DurationMinutes);
                
                await _calendarService.UpdateEventAsync(
                    booking.GoogleCalendarEventId,
                    $"Massage: {booking.Service.Name}",
                    $"Client: {booking.User.FirstName} {booking.User.LastName}\nPhone: {booking.User.PhoneNumber}",
                    booking.BookingDateTime,
                    endTime,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Log but don't fail the booking update if calendar event update fails
                _logger.LogError(ex, "Failed to update calendar event {EventId} for booking {BookingId}", 
                    booking.GoogleCalendarEventId, booking.Id);
            }
        }

        // Send notification about the update
        try
        {
            await _notificationService.SendConfirmationAsync(
                booking.User.TelegramUserId,
                $"📅 Booking updated!\n\nService: {booking.Service.Name}\nNew Date: {booking.BookingDateTime:yyyy-MM-dd HH:mm}\nDuration: {booking.Service.DurationMinutes} min",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send update notification for booking {BookingId}", booking.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
