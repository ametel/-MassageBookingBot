using MassageBookingBot.Application.DTOs;
using MassageBookingBot.Application.Interfaces;
using MassageBookingBot.Domain.Entities;
using MassageBookingBot.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MassageBookingBot.Application.Commands.Bookings;

public record CreateBookingCommand(CreateBookingDto Booking) : IRequest<int>;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICalendarService _calendarService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CreateBookingCommandHandler> _logger;

    public CreateBookingCommandHandler(
        IApplicationDbContext context,
        ICalendarService calendarService,
        INotificationService notificationService,
        ILogger<CreateBookingCommandHandler> logger)
    {
        _context = context;
        _calendarService = calendarService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<int> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services.FindAsync([request.Booking.ServiceId], cancellationToken);
        var user = await _context.Users.FindAsync([request.Booking.UserId], cancellationToken);

        if (service == null || user == null)
            throw new Exception("Service or User not found");

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var booking = new Booking
            {
                UserId = request.Booking.UserId,
                ServiceId = request.Booking.ServiceId,
                BookingDateTime = request.Booking.BookingDateTime,
                Status = BookingStatus.Confirmed,
                Notes = request.Booking.Notes,
                CreatedAt = DateTime.UtcNow,
                ConfirmationSent = false,
                Reminder24hSent = false,
                Reminder2hSent = false
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(cancellationToken);

            // Create Google Calendar event
            var endTime = booking.BookingDateTime.AddMinutes(service.DurationMinutes);
            try
            {
                var eventId = await _calendarService.CreateEventAsync(
                    $"Massage: {service.Name}",
                    $"Client: {user.FirstName} {user.LastName}\nPhone: {user.PhoneNumber}",
                    booking.BookingDateTime,
                    endTime,
                    cancellationToken);

                booking.GoogleCalendarEventId = eventId;
            }
            catch (Exception ex)
            {
                // Log but don't fail the booking if calendar event creation fails
                _logger.LogError(ex, "Failed to create calendar event for booking. Booking will be created without calendar event.");
            }

            // Send confirmation
            try
            {
                await _notificationService.SendConfirmationAsync(
                    user.TelegramUserId,
                    $"✅ Booking confirmed!\n\nService: {service.Name}\nDate: {booking.BookingDateTime:yyyy-MM-dd HH:mm}\nDuration: {service.DurationMinutes} min\nPrice: ${service.Price}",
                    cancellationToken);

                booking.ConfirmationSent = true;
            }
            catch (Exception ex)
            {
                // Log but don't fail the booking if notification fails
                _logger.LogError(ex, "Failed to send confirmation for booking {BookingId}", booking.Id);
            }

            // Save all changes in single transaction
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return booking.Id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
