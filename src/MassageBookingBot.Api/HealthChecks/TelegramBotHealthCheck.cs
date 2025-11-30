using Microsoft.Extensions.Diagnostics.HealthChecks;
using Telegram.Bot;

namespace MassageBookingBot.Api.HealthChecks;

public class TelegramBotHealthCheck : IHealthCheck
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramBotHealthCheck> _logger;

    public TelegramBotHealthCheck(ITelegramBotClient botClient, ILogger<TelegramBotHealthCheck> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var me = await _botClient.GetMe(cancellationToken);
            
            if (me != null && !string.IsNullOrEmpty(me.Username))
            {
                return HealthCheckResult.Healthy($"Telegram bot @{me.Username} is connected");
            }
            
            return HealthCheckResult.Degraded("Telegram bot connected but response is incomplete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telegram bot health check failed");
            return HealthCheckResult.Unhealthy("Telegram bot is not responding", ex);
        }
    }
}
