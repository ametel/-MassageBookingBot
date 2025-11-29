namespace MassageBookingBot.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public long TelegramUserId { get; set; }
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ReferralCode { get; set; }
    public int ReferralCount { get; set; }
    public decimal DiscountBalance { get; set; }
}
