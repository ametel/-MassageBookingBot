using MassageBookingBot.Domain.Common;

namespace MassageBookingBot.Domain.Entities;

public class Service : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
