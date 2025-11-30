using MassageBookingBot.Domain.Common;

namespace MassageBookingBot.Domain.Entities;

public class User : BaseEntity
{
    public long TelegramUserId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ReferralCode { get; set; }
    public string? ReferredByCode { get; set; }
    public int ReferralCount { get; set; }
    public decimal DiscountBalance { get; set; }
    
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<UserState> States { get; set; } = new List<UserState>();
}
