using MassageBookingBot.Application;
using MassageBookingBot.Infrastructure;
using MassageBookingBot.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application layer (includes validators)
builder.Services.AddApplication();

// Add MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(MassageBookingBot.Application.Queries.Services.GetServicesQuery).Assembly);
});

// Add Telegram Bot (optional for API - needed for notification service)
var botToken = builder.Configuration["TelegramBot:Token"] ?? "DUMMY_TOKEN";
builder.Services.AddSingleton<Telegram.Bot.ITelegramBotClient>(sp => 
{
    try 
    {
        return new Telegram.Bot.TelegramBotClient(botToken);
    }
    catch
    {
        // If token is invalid, return a dummy client
        return null!;
    }
});

// Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Validate JWT configuration in production
if (builder.Environment.IsProduction())
{
    if (string.IsNullOrEmpty(jwtKey))
        throw new InvalidOperationException("JWT Key must be configured in production. Set 'Jwt:Key' in configuration.");
    if (jwtKey.Length < 32)
        throw new InvalidOperationException("JWT Key must be at least 32 characters long for security.");
}

// Use defaults only in development
jwtKey ??= "DevelopmentKeyOnlyNotForProduction123456789ABCDEF";
jwtIssuer ??= "MassageBookingBot";
jwtAudience ??= "MassageBookingBot";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database")
    .AddCheck<MassageBookingBot.Api.HealthChecks.TelegramBotHealthCheck>("telegram-bot");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    MassageBookingBot.Infrastructure.Persistence.DbInitializer.Initialize(context);
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map health checks endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
