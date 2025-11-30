using MassageBookingBot.BotWorker.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace MassageBookingBot.BotWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;

    public Worker(
        ILogger<Worker> logger,
        ITelegramBotClient botClient,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _botClient = botClient;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Bot Worker starting...");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            stoppingToken
        );

        try
        {
            var me = await _botClient.GetMe(stoppingToken);
            _logger.LogInformation("Bot @{Username} is running", me.Username);
            
            // Set bot commands menu
            await SetBotCommandsAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify bot token. Check your Telegram Bot token configuration.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<BotUpdateHandler>();
        await handler.HandleUpdateAsync(update, cancellationToken);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error in Telegram Bot");
        return Task.CompletedTask;
    }

    private async Task SetBotCommandsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var commands = new[]
            {
                new BotCommand { Command = "start", Description = "start the bot and register" },
                new BotCommand { Command = "book", Description = "book a massage appointment" },
                new BotCommand { Command = "mybookings", Description = "view your bookings" },
                new BotCommand { Command = "cancel", Description = "cancel a booking" },
                new BotCommand { Command = "help", Description = "show help message" }
            };

            await _botClient.SetMyCommands(commands, cancellationToken: cancellationToken);
            _logger.LogInformation("Bot commands menu set successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set bot commands menu");
        }
    }
}
