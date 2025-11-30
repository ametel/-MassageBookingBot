using MassageBookingBot.Domain.Common;

namespace MassageBookingBot.Domain.Entities;

public class TimeSlot : BaseEntity
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsBooked { get; set; }
    public int? BookingId { get; set; }
}
