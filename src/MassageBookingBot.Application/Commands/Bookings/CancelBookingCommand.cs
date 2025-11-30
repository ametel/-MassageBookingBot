using MassageBookingBot.Application.Interfaces;
using MassageBookingBot.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MassageBookingBot.Application.Commands.Bookings;

public record CancelBookingCommand(int BookingId) : IRequest<bool>;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalendarService _calendarService;

    public CancelBookingCommandHandler(IApplicationDbContext context, ICalendarService calendarService)
    {
        _context = context;
        _calendarService = calendarService;
    }

    public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings.FindAsync([request.BookingId], cancellationToken);
        
        if (booking == null)
            return false;

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(booking.GoogleCalendarEventId))
        {
            await _calendarService.DeleteEventAsync(booking.GoogleCalendarEventId, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
