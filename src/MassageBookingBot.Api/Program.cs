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
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MySecretKeyForJwtTokenGeneration123456789";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MassageBookingBot";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MassageBookingBot";

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
app.MapControllers();

app.Run();
