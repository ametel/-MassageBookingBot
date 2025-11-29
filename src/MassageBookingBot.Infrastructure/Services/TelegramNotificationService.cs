using MassageBookingBot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace MassageBookingBot.Infrastructure.Services;

public class TelegramNotificationService : INotificationService
{
    private readonly ITelegramBotClient _botClient;

    public TelegramNotificationService(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task SendConfirmationAsync(long telegramUserId, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _botClient.SendMessage(telegramUserId, message, cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            // Log error
        }
    }

    public async Task SendReminderAsync(long telegramUserId, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _botClient.SendMessage(telegramUserId, message, cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            // Log error
        }
    }
}
