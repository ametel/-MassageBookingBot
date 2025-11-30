using MassageBookingBot.Application.Interfaces;
using MassageBookingBot.Domain.Entities;
using MassageBookingBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MassageBookingBot.BotWorker.Services;

public class BotUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<BotUpdateHandler> _logger;

    public BotUpdateHandler(
        ITelegramBotClient botClient,
        IApplicationDbContext context,
        ILogger<BotUpdateHandler> logger)
    {
        _botClient = botClient;
        _context = context;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandleMessageAsync(update.Message, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await HandleCallbackQueryAsync(update.CallbackQuery, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update");
        }
    }

    private async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        var text = message.Text!;
        var chatId = message.Chat.Id;

        if (text == "/start")
        {
            await HandleStartCommandAsync(message, cancellationToken);
            return;
        }

        if (text == "/book")
        {
            await ShowServicesAsync(chatId, cancellationToken);
            return;
        }

        if (text == "/mybookings")
        {
            await ShowUserBookingsAsync(chatId, cancellationToken);
            return;
        }

        if (text == "/help")
        {
            await SendHelpMessageAsync(chatId, cancellationToken);
            return;
        }
    }

    private async Task HandleStartCommandAsync(Message message, CancellationToken cancellationToken)
    {
        var chatId = message.Chat.Id;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == chatId, cancellationToken);

        if (user == null)
        {
            user = new Domain.Entities.User
            {
                TelegramUserId = chatId,
                Username = message.From?.Username,
                FirstName = message.From?.FirstName,
                LastName = message.From?.LastName,
                ReferralCode = GenerateReferralCode(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var welcomeMessage = $"Welcome {user.FirstName}! 👋\n\n" +
                            "I can help you book massage appointments.\n\n" +
                            "Available commands:\n" +
                            "/book - Book a massage\n" +
                            "/mybookings - View your bookings\n" +
                            "/help - Get help\n\n" +
                            $"Your referral code: {user.ReferralCode}\n" +
                            $"Share it with friends to earn discounts!";

        await _botClient.SendMessage(chatId, welcomeMessage, cancellationToken: cancellationToken);
    }

    private async Task ShowServicesAsync(long chatId, CancellationToken cancellationToken)
    {
        var services = await _context.Services
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

        if (!services.Any())
        {
            await _botClient.SendMessage(chatId, "No services available at the moment.", cancellationToken: cancellationToken);
            return;
        }

        var keyboard = new InlineKeyboardMarkup(
            services.Select(s => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{s.Name} - ${s.Price} ({s.DurationMinutes} min)",
                    $"service_{s.Id}")
            })
        );

        await _botClient.SendMessage(
            chatId,
            "Please select a service:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task ShowUserBookingsAsync(long chatId, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == chatId, cancellationToken);
        if (user == null) return;

        var bookings = await _context.Bookings
            .Include(b => b.Service)
            .Where(b => b.UserId == user.Id && b.Status == BookingStatus.Confirmed)
            .OrderBy(b => b.BookingDateTime)
            .ToListAsync(cancellationToken);

        if (!bookings.Any())
        {
            await _botClient.SendMessage(chatId, "You have no active bookings.", cancellationToken: cancellationToken);
            return;
        }

        var message = "Your bookings:\n\n";
        foreach (var booking in bookings)
        {
            message += $"📅 {booking.BookingDateTime:yyyy-MM-dd HH:mm}\n" +
                      $"💆 {booking.Service.Name}\n" +
                      $"⏱ {booking.Service.DurationMinutes} min\n" +
                      $"💰 ${booking.Service.Price}\n\n";
        }

        await _botClient.SendMessage(chatId, message, cancellationToken: cancellationToken);
    }

    private async Task SendHelpMessageAsync(long chatId, CancellationToken cancellationToken)
    {
        var helpMessage = "📖 Help\n\n" +
                         "Available commands:\n" +
                         "/start - Start the bot\n" +
                         "/book - Book a massage appointment\n" +
                         "/mybookings - View your bookings\n" +
                         "/help - Show this help message\n\n" +
                         "To book an appointment, use /book and follow the steps.";

        await _botClient.SendMessage(chatId, helpMessage, cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var data = callbackQuery.Data!;

        if (data.StartsWith("service_"))
        {
            var serviceId = int.Parse(data.Replace("service_", ""));
            await ShowAvailableDatesAsync(chatId, serviceId, cancellationToken);
        }

        await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
    }

    private async Task ShowAvailableDatesAsync(long chatId, int serviceId, CancellationToken cancellationToken)
    {
        var dates = new List<DateTime>();
        for (int i = 1; i <= 7; i++)
        {
            dates.Add(DateTime.Today.AddDays(i));
        }

        var keyboard = new InlineKeyboardMarkup(
            dates.Select(d => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    d.ToString("yyyy-MM-dd"),
                    $"date_{serviceId}_{d:yyyyMMdd}")
            })
        );

        await _botClient.SendMessage(
            chatId,
            "Please select a date:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private string GenerateReferralCode()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}
