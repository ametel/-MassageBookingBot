namespace MassageBookingBot.Application.Interfaces;

public interface INotificationService
{
    Task SendConfirmationAsync(long telegramUserId, string message, CancellationToken cancellationToken = default);
    Task SendReminderAsync(long telegramUserId, string message, CancellationToken cancellationToken = default);
}
