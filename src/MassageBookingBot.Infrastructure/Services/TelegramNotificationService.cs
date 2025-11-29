using MassageBookingBot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace MassageBookingBot.Infrastructure.Services;

public class TelegramNotificationService : INotificationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(ITelegramBotClient botClient, ILogger<TelegramNotificationService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task SendConfirmationAsync(long telegramUserId, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _botClient.SendMessage(telegramUserId, message, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation to user {UserId}", telegramUserId);
        }
    }

    public async Task SendReminderAsync(long telegramUserId, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _botClient.SendMessage(telegramUserId, message, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder to user {UserId}", telegramUserId);
        }
    }
}
