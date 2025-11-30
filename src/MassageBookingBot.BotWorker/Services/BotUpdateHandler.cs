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
            _logger.LogDebug("Received update type: {UpdateType}", update.Type);
            
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                _logger.LogInformation("Processing message from user {UserId}: {Text}", 
                    update.Message.From?.Id, update.Message.Text);
                await HandleMessageAsync(update.Message, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                _logger.LogInformation("Processing callback query from user {UserId}: {Data}", 
                    update.CallbackQuery.From.Id, update.CallbackQuery.Data);
                await HandleCallbackQueryAsync(update.CallbackQuery, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling update {UpdateId} of type {UpdateType}", 
                update.Id, update.Type);
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

        if (text == "/cancel")
        {
            await ShowCancelBookingMenuAsync(chatId, cancellationToken);
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
        _logger.LogInformation("Handling /start command for user {UserId} (@{Username})", 
            chatId, message.From?.Username);
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == chatId, cancellationToken);

        if (user == null)
        {
            _logger.LogInformation("Creating new user {UserId} with username @{Username}", 
                chatId, message.From?.Username);
            
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
            
            _logger.LogInformation("User {UserId} created successfully with referral code {ReferralCode}", 
                user.Id, user.ReferralCode);
        }
        else
        {
            _logger.LogDebug("Existing user {UserId} with referral code {ReferralCode}", 
                user.Id, user.ReferralCode);
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
        _logger.LogInformation("Welcome message sent to user {UserId}", chatId);
    }

    private async Task ShowServicesAsync(long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Showing services to user {UserId}", chatId);
        
        var services = await _context.Services
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {ServiceCount} active services", services.Count);

        if (!services.Any())
        {
            _logger.LogWarning("No active services available for user {UserId}", chatId);
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
        
        _logger.LogInformation("Service selection menu sent to user {UserId}", chatId);
    }

    private async Task ShowUserBookingsAsync(long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Showing bookings for user {UserId}", chatId);
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == chatId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found in database", chatId);
            return;
        }

        var bookings = await _context.Bookings
            .Include(b => b.Service)
            .Where(b => b.UserId == user.Id && b.Status == BookingStatus.Confirmed)
            .OrderBy(b => b.BookingDateTime)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("User {UserId} has {BookingCount} active bookings", user.Id, bookings.Count);

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
        _logger.LogInformation("Bookings list sent to user {UserId}", chatId);
    }

    private async Task ShowCancelBookingMenuAsync(long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Showing cancel booking menu for user {UserId}", chatId);
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == chatId, cancellationToken);
        if (user == null)
        {
            await _botClient.SendMessage(chatId, "Please use /start first to register.", cancellationToken: cancellationToken);
            return;
        }

        var bookings = await _context.Bookings
            .Include(b => b.Service)
            .Where(b => b.UserId == user.Id && b.Status == BookingStatus.Confirmed)
            .OrderBy(b => b.BookingDateTime)
            .ToListAsync(cancellationToken);

        if (!bookings.Any())
        {
            await _botClient.SendMessage(chatId, "You have no active bookings to cancel.", cancellationToken: cancellationToken);
            return;
        }

        var buttons = new List<List<InlineKeyboardButton>>();
        foreach (var booking in bookings)
        {
            var buttonText = $"{booking.BookingDateTime:MMM dd, HH:mm} - {booking.Service.Name}";
            buttons.Add([InlineKeyboardButton.WithCallbackData(buttonText, $"cancelbook:{booking.Id}")]);
        }

        var keyboard = new InlineKeyboardMarkup(buttons);
        await _botClient.SendMessage(
            chatId,
            "Select a booking to cancel:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        
        _logger.LogInformation("Cancel booking menu sent to user {UserId} with {BookingCount} bookings", chatId, bookings.Count);
    }

    private async Task SendHelpMessageAsync(long chatId, CancellationToken cancellationToken)
    {
        var helpMessage = "📖 *Available Commands*\n\n" +
                         "/start - start the bot and register\n" +
                         "/book - book a massage appointment\n" +
                         "/mybookings - view your bookings\n" +
                         "/cancel - cancel a booking\n" +
                         "/help - show this help message\n\n" +
                         "💆‍♀️ *How to book:*\n" +
                         "1. Use /book command\n" +
                         "2. Select a massage service\n" +
                         "3. Choose your preferred date\n" +
                         "4. Pick an available time slot\n" +
                         "5. Confirm your booking\n\n" +
                         "You'll receive reminder notifications 24 hours and 2 hours before your appointment.";

        await _botClient.SendMessage(
            chatId, 
            helpMessage, 
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

    private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var data = callbackQuery.Data!;

        _logger.LogDebug("Handling callback query: {Data} from user {UserId}", data, chatId);

        // Check if the message still has a keyboard (not already processed)
        if (callbackQuery.Message.ReplyMarkup == null || !callbackQuery.Message.ReplyMarkup.InlineKeyboard.Any())
        {
            _logger.LogDebug("Ignoring callback from old message without keyboard for user {UserId}", chatId);
            await _botClient.AnswerCallbackQuery(
                callbackQuery.Id, 
                "This selection has already been processed. Please start a new booking with /book",
                showAlert: true,
                cancellationToken: cancellationToken);
            return;
        }

        if (data.StartsWith("service_"))
        {
            var serviceId = int.Parse(data.Replace("service_", ""));
            _logger.LogInformation("User {UserId} selected service {ServiceId}", chatId, serviceId);
            await ShowAvailableDatesAsync(chatId, serviceId, cancellationToken);
        }
        else if (data.StartsWith("date_"))
        {
            // Format: date_serviceId_yyyyMMdd
            var parts = data.Split('_');
            var serviceId = int.Parse(parts[1]);
            var dateStr = parts[2];
            var selectedDate = DateTime.ParseExact(dateStr, "yyyyMMdd", null);
            
            _logger.LogInformation("User {UserId} selected date {Date} for service {ServiceId}", 
                chatId, selectedDate, serviceId);
            await ShowAvailableTimeSlotsAsync(chatId, serviceId, selectedDate, callbackQuery, cancellationToken);
        }
        else if (data.StartsWith("time_"))
        {
            // Format: time_serviceId_yyyyMMdd_HHmm
            var parts = data.Split('_');
            var serviceId = int.Parse(parts[1]);
            var dateStr = parts[2];
            var timeStr = parts[3];
            var selectedDate = DateTime.ParseExact(dateStr, "yyyyMMdd", null);
            var selectedTime = TimeSpan.ParseExact(timeStr, "hhmm", null);
            var bookingDateTime = selectedDate.Add(selectedTime);
            
            _logger.LogInformation("User {UserId} selected time {Time} for service {ServiceId}", 
                chatId, bookingDateTime, serviceId);
            await ConfirmBookingAsync(chatId, serviceId, bookingDateTime, callbackQuery, cancellationToken);
        }
        else if (data.StartsWith("confirm_"))
        {
            // Format: confirm_serviceId_yyyyMMddHHmm
            var parts = data.Split('_');
            var serviceId = int.Parse(parts[1]);
            var dateTimeStr = parts[2];
            var bookingDateTime = DateTime.ParseExact(dateTimeStr, "yyyyMMddHHmm", null);
            
            _logger.LogInformation("User {UserId} confirming booking for service {ServiceId} at {DateTime}", 
                chatId, serviceId, bookingDateTime);
            await CreateBookingAsync(chatId, serviceId, bookingDateTime, callbackQuery, cancellationToken);
        }
        else if (data.StartsWith("cancelbook:"))
        {
            var bookingId = int.Parse(data.Replace("cancelbook:", ""));
            _logger.LogInformation("User {UserId} requested to cancel booking {BookingId}", chatId, bookingId);
            await CancelBookingAsync(chatId, bookingId, callbackQuery, cancellationToken);
        }
        else if (data == "cancel_booking")
        {
            _logger.LogInformation("User {UserId} cancelled booking", chatId);
            await _botClient.SendMessage(chatId, "Booking cancelled. Use /book to start a new booking.", cancellationToken: cancellationToken);
            
            // Remove keyboard from the confirmation message
            try
            {
                await _botClient.EditMessageReplyMarkup(
                    chatId,
                    callbackQuery.Message!.MessageId,
                    replyMarkup: null,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to remove keyboard from confirmation message");
            }
        }

        await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
    }

    private async Task ShowAvailableDatesAsync(long chatId, int serviceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Showing available dates for service {ServiceId} to user {UserId}", serviceId, chatId);
        
        var dates = new List<DateTime>();
        for (int i = 1; i <= 7; i++)
        {
            dates.Add(DateTime.Today.AddDays(i));
        }

        _logger.LogDebug("Generated {DateCount} date options for next 7 days", dates.Count);

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
        
        _logger.LogInformation("Date selection menu sent to user {UserId}", chatId);
    }

    private async Task ShowAvailableTimeSlotsAsync(long chatId, int serviceId, DateTime selectedDate, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Showing available time slots for service {ServiceId} on {Date} to user {UserId}", 
            serviceId, selectedDate, chatId);

        var startOfDay = selectedDate.Date;
        var endOfDay = startOfDay.AddDays(1);

        var availableSlots = await _context.TimeSlots
            .Where(t => t.StartTime >= startOfDay && t.StartTime < endOfDay && t.IsAvailable && !t.IsBooked)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {SlotCount} available time slots", availableSlots.Count);

        if (!availableSlots.Any())
        {
            _logger.LogWarning("No available time slots for service {ServiceId} on {Date}", serviceId, selectedDate);
            await _botClient.SendMessage(chatId, "No available time slots for this date. Please select another date.", cancellationToken: cancellationToken);
            await ShowAvailableDatesAsync(chatId, serviceId, cancellationToken);
            return;
        }

        var keyboard = new InlineKeyboardMarkup(
            availableSlots.Select(slot => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    slot.StartTime.ToString("HH:mm"),
                    $"time_{serviceId}_{selectedDate:yyyyMMdd}_{slot.StartTime:HHmm}")
            })
        );

        await _botClient.SendMessage(
            chatId,
            $"Please select a time for {selectedDate:yyyy-MM-dd}:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        
        _logger.LogInformation("Time slot selection menu sent to user {UserId}", chatId);
        
        // Remove keyboard from the date selection message
        try
        {
            await _botClient.EditMessageReplyMarkup(
                chatId,
                callbackQuery.Message!.MessageId,
                replyMarkup: null,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove keyboard from date selection message");
        }
    }

    private async Task ConfirmBookingAsync(long chatId, int serviceId, DateTime bookingDateTime, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Showing booking confirmation to user {UserId} for service {ServiceId} at {DateTime}", 
            chatId, serviceId, bookingDateTime);

        var service = await _context.Services.FindAsync(new object[] { serviceId }, cancellationToken);
        if (service == null)
        {
            _logger.LogWarning("Service {ServiceId} not found", serviceId);
            await _botClient.SendMessage(chatId, "Service not found. Please try again.", cancellationToken: cancellationToken);
            return;
        }

        var confirmationMessage = $"📋 Booking Summary:\n\n" +
                                 $"💆 Service: {service.Name}\n" +
                                 $"📅 Date: {bookingDateTime:yyyy-MM-dd}\n" +
                                 $"⏰ Time: {bookingDateTime:HH:mm}\n" +
                                 $"⏱️ Duration: {service.DurationMinutes} minutes\n" +
                                 $"💰 Price: ${service.Price}\n\n" +
                                 $"Please confirm your booking:";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Confirm", $"confirm_{serviceId}_{bookingDateTime:yyyyMMddHHmm}"),
                InlineKeyboardButton.WithCallbackData("❌ Cancel", "cancel_booking")
            }
        });

        var sentMessage = await _botClient.SendMessage(
            chatId,
            confirmationMessage,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
        
        _logger.LogInformation("Confirmation prompt sent to user {UserId}", chatId);
        
        // Remove keyboard from the time selection message
        try
        {
            await _botClient.EditMessageReplyMarkup(
                chatId,
                callbackQuery.Message!.MessageId,
                replyMarkup: null,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove keyboard from time selection message");
        }
    }

    private async Task CreateBookingAsync(long chatId, int serviceId, DateTime bookingDateTime, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating booking for user {UserId}, service {ServiceId} at {DateTime}", 
            chatId, serviceId, bookingDateTime);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == chatId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", chatId);
            await _botClient.SendMessage(chatId, "User not found. Please use /start first.", cancellationToken: cancellationToken);
            return;
        }

        var service = await _context.Services.FindAsync(new object[] { serviceId }, cancellationToken);
        if (service == null)
        {
            _logger.LogWarning("Service {ServiceId} not found", serviceId);
            await _botClient.SendMessage(chatId, "Service not found. Please try again.", cancellationToken: cancellationToken);
            return;
        }

        // Use transaction to prevent race conditions
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Check for duplicate booking
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == user.Id 
                    && b.ServiceId == serviceId 
                    && b.BookingDateTime == bookingDateTime 
                    && b.Status == BookingStatus.Confirmed, cancellationToken);

            if (existingBooking != null)
            {
                _logger.LogWarning("Duplicate booking attempt prevented for user {UserId}, booking {BookingId}", user.Id, existingBooking.Id);
                await _botClient.SendMessage(chatId, "You already have a booking for this time slot. Use /mybookings to see your appointments.", cancellationToken: cancellationToken);
                return;
            }

            // Find and lock the time slot to prevent concurrent bookings
            var timeSlot = await _context.TimeSlots
                .Where(t => t.StartTime == bookingDateTime && t.IsAvailable && !t.IsBooked)
                .FirstOrDefaultAsync(cancellationToken);

            if (timeSlot == null)
            {
                _logger.LogWarning("Time slot not available for {DateTime}", bookingDateTime);
                await _botClient.SendMessage(chatId, "This time slot is no longer available. Please select another time.", cancellationToken: cancellationToken);
                return;
            }

            // Create the booking and mark slot as booked in single transaction
            var booking = new Booking
            {
                UserId = user.Id,
                ServiceId = serviceId,
                BookingDateTime = bookingDateTime,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            
            // Mark time slot as booked
            timeSlot.IsBooked = true;
            timeSlot.BookingId = booking.Id;
            
            // Save all changes in one transaction
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Booking {BookingId} created successfully for user {UserId}", booking.Id, user.Id);
            
            var successMessage = $"🎉 Booking Confirmed!\n\n" +
                               $"Your appointment has been successfully booked:\n\n" +
                               $"💆 {service.Name}\n" +
                               $"📅 {bookingDateTime:yyyy-MM-dd}\n" +
                               $"⏰ {bookingDateTime:HH:mm}\n" +
                               $"⏱️ {service.DurationMinutes} minutes\n" +
                               $"💰 ${service.Price}\n\n" +
                               $"You will receive:\n" +
                               $"- A reminder 24 hours before your appointment\n" +
                               $"- A reminder 2 hours before your appointment\n\n" +
                               $"See you soon! 💆‍♀️\n\n" +
                               $"Use /mybookings to see all your bookings.";

            await _botClient.SendMessage(chatId, successMessage, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Confirmation message sent to user {UserId} for booking {BookingId}", chatId, booking.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create booking for user {UserId} at {DateTime}", user.Id, bookingDateTime);
            await _botClient.SendMessage(chatId, "An error occurred while creating your booking. Please try again.", cancellationToken: cancellationToken);
            return;
        }
        
        // Remove keyboard from the confirmation message
        try
        {
            await _botClient.EditMessageReplyMarkup(
                chatId,
                callbackQuery.Message!.MessageId,
                replyMarkup: null,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove keyboard from confirmation message");
        }
    }

    private async Task CancelBookingAsync(long chatId, int bookingId, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling booking {BookingId} for user {UserId}", bookingId, chatId);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == chatId, cancellationToken);
        if (user == null)
        {
            await _botClient.SendMessage(chatId, "User not found. Please use /start first.", cancellationToken: cancellationToken);
            return;
        }

        var booking = await _context.Bookings
            .Include(b => b.Service)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user.Id, cancellationToken);

        if (booking == null)
        {
            await _botClient.SendMessage(chatId, "Booking not found.", cancellationToken: cancellationToken);
            return;
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            await _botClient.SendMessage(chatId, "This booking has already been cancelled.", cancellationToken: cancellationToken);
            return;
        }

        // Update booking status
        booking.Status = BookingStatus.Cancelled;

        // Free up the time slot
        var timeSlot = await _context.TimeSlots
            .FirstOrDefaultAsync(t => t.BookingId == bookingId, cancellationToken);
        
        if (timeSlot != null)
        {
            timeSlot.IsBooked = false;
            timeSlot.BookingId = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var message = $"❌ Booking Cancelled\n\n" +
                     $"Your appointment has been cancelled:\n\n" +
                     $"💆 {booking.Service.Name}\n" +
                     $"📅 {booking.BookingDateTime:yyyy-MM-dd HH:mm}\n\n" +
                     $"Feel free to book again anytime with /book";

        await _botClient.SendMessage(chatId, message, cancellationToken: cancellationToken);
        
        // Remove keyboard from the cancellation menu
        try
        {
            await _botClient.EditMessageReplyMarkup(
                chatId,
                callbackQuery.Message!.MessageId,
                replyMarkup: null,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove keyboard from cancellation message");
        }
        
        _logger.LogInformation("Booking {BookingId} cancelled successfully", bookingId);
    }

    private string GenerateReferralCode()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }
}
