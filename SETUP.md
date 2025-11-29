# Setup Guide for Massage Booking Bot System

This guide will walk you through setting up the entire Massage Booking Bot system.

## Prerequisites

Before you begin, ensure you have the following installed:

- .NET 8.0 SDK or later ([Download](https://dotnet.microsoft.com/download))
- Node.js 18+ and npm ([Download](https://nodejs.org/))
- A Telegram account
- (Optional) Google Cloud account for Calendar API

## Step 1: Create Your Telegram Bot

1. Open Telegram and search for [@BotFather](https://t.me/botfather)
2. Send `/newbot` command
3. Follow the prompts to create your bot:
   - Choose a name for your bot
   - Choose a username (must end with 'bot')
4. Copy the bot token provided by BotFather
5. Save this token - you'll need it in the next step

## Step 2: Configure the Application

### 2.1 API Configuration

Edit `src/MassageBookingBot.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=massagebooking.db"
  },
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN_HERE"
  },
  "Jwt": {
    "Key": "CHANGE_THIS_TO_A_SECURE_KEY_AT_LEAST_32_CHARACTERS",
    "Issuer": "MassageBookingBot",
    "Audience": "MassageBookingBot",
    "ExpiryMinutes": 1440
  }
}
```

### 2.2 Bot Worker Configuration

Edit `src/MassageBookingBot.BotWorker/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=massagebooking.db"
  },
  "TelegramBot": {
    "Token": "YOUR_BOT_TOKEN_HERE"
  }
}
```

### 2.3 Admin Panel Configuration

Edit `admin-panel/src/app/services/api.service.ts`:

```typescript
private apiUrl = 'http://localhost:5000/api'; // Change for production
```

## Step 3: Build and Run the Backend

### 3.1 Restore Dependencies

```bash
cd /path/to/-MassageBookingBot
dotnet restore
```

### 3.2 Build the Solution

```bash
dotnet build
```

### 3.3 Run the API

```bash
cd src/MassageBookingBot.Api
dotnet run
```

The API will start at `http://localhost:5000`
Swagger UI will be available at `http://localhost:5000/swagger`

### 3.4 Run the Bot Worker (in a new terminal)

```bash
cd src/MassageBookingBot.BotWorker
dotnet run
```

The bot will start polling for Telegram updates.

## Step 4: Run the Admin Panel

```bash
cd admin-panel
npm install
npm start
```

The admin panel will be available at `http://localhost:4200`

## Step 5: Test Your Bot

1. Open Telegram
2. Search for your bot by username
3. Send `/start` to begin
4. Try the following commands:
   - `/book` - Start booking a massage
   - `/mybookings` - View your bookings
   - `/help` - Get help

## Step 6: Access the Admin Panel

1. Open your browser and navigate to `http://localhost:4200`
2. You'll see the dashboard with:
   - Dashboard - Overview metrics
   - Bookings - View and manage all bookings
   - Services - View all available services
   - Clients - Client management (placeholder)

## Database Seeding

The database is automatically seeded with sample data on first run:

- **5 massage services**: Swedish, Deep Tissue, Hot Stone, Sports, Aromatherapy
- **56 time slots**: 7 days × 8 hours (9 AM - 5 PM)

## Optional: Google Calendar Integration

To enable real Google Calendar integration:

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google Calendar API
4. Create OAuth 2.0 credentials
5. Download the credentials as `credentials.json`
6. Place the file in `src/MassageBookingBot.Api/`
7. Update `GoogleCalendarService.cs` to use real API calls

## Production Deployment

### Environment Variables

Set these environment variables in production:

```bash
ConnectionStrings__DefaultConnection="Data Source=/path/to/massagebooking.db"
TelegramBot__Token="YOUR_PRODUCTION_BOT_TOKEN"
Jwt__Key="YOUR_SECURE_JWT_KEY_AT_LEAST_32_CHARACTERS"
```

### HTTPS Configuration

Update `Program.cs` to enable HTTPS:

```csharp
app.UseHttpsRedirection();
```

### CORS Configuration

Update CORS settings in `Program.cs` for production:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Database

- Consider migrating from SQLite to PostgreSQL or SQL Server for production
- Implement proper database migrations using EF Core Migrations

### Docker Deployment (Optional)

Create `Dockerfile` for each service:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MassageBookingBot.Api.dll"]
```

## Troubleshooting

### Bot Not Responding

1. Check that the bot token is correct
2. Verify the Bot Worker is running
3. Check logs for any errors

### API Connection Issues

1. Verify the API is running on the correct port
2. Check CORS settings
3. Ensure firewall allows the connection

### Database Issues

1. Delete `massagebooking.db` and restart to recreate
2. Check file permissions
3. Verify the connection string

## Security Checklist

- [ ] Change the default JWT secret key
- [ ] Enable HTTPS in production
- [ ] Implement proper authentication for admin panel
- [ ] Use environment variables for sensitive data
- [ ] Enable rate limiting
- [ ] Implement proper error handling
- [ ] Add logging and monitoring

## Support

For issues and questions:
- Check the [README.md](README.md)
- Open an issue on GitHub
- Review the code documentation

## Next Steps

- Implement user authentication for the admin panel
- Add more payment gateway integration
- Implement email notifications
- Add analytics and reporting
- Create mobile app version
