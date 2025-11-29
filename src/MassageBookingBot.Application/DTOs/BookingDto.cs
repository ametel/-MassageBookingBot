using MassageBookingBot.Domain.Enums;

namespace MassageBookingBot.Application.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public DateTime BookingDateTime { get; set; }
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class CreateBookingDto
{
    public int UserId { get; set; }
    public int ServiceId { get; set; }
    public DateTime BookingDateTime { get; set; }
    public string? Notes { get; set; }
}
