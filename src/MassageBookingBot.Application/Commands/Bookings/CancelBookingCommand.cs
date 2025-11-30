using MassageBookingBot.Application.Interfaces;
using MassageBookingBot.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MassageBookingBot.Application.Commands.Bookings;

public record CancelBookingCommand(int BookingId) : IRequest<bool>;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalendarService _calendarService;
    private readonly ILogger<CancelBookingCommandHandler> _logger;

    public CancelBookingCommandHandler(
        IApplicationDbContext context, 
        ICalendarService calendarService,
        ILogger<CancelBookingCommandHandler> logger)
    {
        _context = context;
        _calendarService = calendarService;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings.FindAsync([request.BookingId], cancellationToken);
        
        if (booking == null)
            return false;

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        // Try to delete calendar event, but don't fail booking cancellation if it fails
        if (!string.IsNullOrEmpty(booking.GoogleCalendarEventId))
        {
            try
            {
                await _calendarService.DeleteEventAsync(booking.GoogleCalendarEventId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete calendar event {EventId} for booking {BookingId}. Booking will still be cancelled.", 
                    booking.GoogleCalendarEventId, booking.Id);
                // Continue with cancellation even if calendar deletion fails
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
