namespace MassageBookingBot.Domain.Enums;

public enum BotState
{
    Start,
    BrowsingServices,
    SelectingDate,
    SelectingTime,
    ConfirmingBooking,
    EnteringName,
    EnteringPhone,
    ManagingBookings,
    EnteringReferralCode
}
