using MassageBookingBot.Domain.Common;
using MassageBookingBot.Domain.Enums;

namespace MassageBookingBot.Domain.Entities;

public class Booking : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    
    public DateTime BookingDateTime { get; set; }
    public BookingStatus Status { get; set; }
    public string? GoogleCalendarEventId { get; set; }
    public bool ConfirmationSent { get; set; }
    public bool Reminder24hSent { get; set; }
    public bool Reminder2hSent { get; set; }
    public string? Notes { get; set; }
}
