using MassageBookingBot.Domain.Common;
using MassageBookingBot.Domain.Enums;

namespace MassageBookingBot.Domain.Entities;

public class UserState : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public BotState State { get; set; }
    public string? StateData { get; set; }
}
