using MassageBookingBot.BotWorker;
using MassageBookingBot.BotWorker.Services;
using MassageBookingBot.Infrastructure;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

// Add Telegram Bot
var botToken = builder.Configuration["TelegramBot:Token"] ?? throw new InvalidOperationException("Telegram Bot Token not configured");
builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Add Bot Services
builder.Services.AddScoped<BotUpdateHandler>();

// Add Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
