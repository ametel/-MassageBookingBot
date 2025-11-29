# Massage Booking Bot System

A comprehensive massage booking system with Telegram bot integration, admin web panel, and automated notifications.

## Features

### 1. Client-Facing Telegram Bot
- ✅ Browse available massage services
- ✅ Select date and time from available schedule slots
- ✅ Book appointments with confirmation
- ✅ Automatic notifications (instant confirmation, 24h reminder, 2h reminder)
- ✅ Cancel or reschedule appointments
- ✅ Referral system with unique codes and discounts
- ✅ User profile management (name, phone)
- ✅ Multi-step guided booking flow with FSM (Finite State Machine)

### 2. Scheduling & Calendar Management
- ✅ Therapist-defined available time slots
- ✅ Automatic booking marking
- ✅ Google Calendar integration (create, update, delete events)

### 3. Admin Web Panel (Angular)
- ✅ View all bookings with filters and search
- ✅ Manage services (price, duration, description)
- ✅ Manage schedule slots
- ✅ Manage clients
- ✅ Referral statistics
- ✅ Dashboard with metrics

### 4. Backend API (ASP.NET Core)
- ✅ Clean Architecture structure
- ✅ EF Core + SQLite storage
- ✅ CQRS with MediatR
- ✅ Quartz.NET scheduled jobs for reminders
- ✅ JWT authentication for admin section
- ✅ Swagger/OpenAPI documentation

### 5. Bot Worker Service
- ✅ Background worker polling Telegram API
- ✅ Process user interactions and commands
- ✅ FSM with persistent state in database
- ✅ Inline keyboards for choosing services/dates/times

## Architecture

```
/src
  /MassageBookingBot.Domain          - Domain entities and enums
  /MassageBookingBot.Application     - CQRS commands/queries, interfaces
  /MassageBookingBot.Infrastructure  - EF Core, services, Quartz jobs
  /MassageBookingBot.Api            - REST API with JWT auth
  /MassageBookingBot.BotWorker      - Telegram bot worker service
/admin-panel                         - Angular admin web application
```

## Prerequisites

- .NET 8.0 SDK or later
- Node.js 18+ and npm (for Angular admin panel)
- SQLite
- Telegram Bot Token (from @BotFather)
- (Optional) Google Calendar API credentials

## Setup

### 1. Configure Telegram Bot

1. Create a bot via [@BotFather](https://t.me/botfather) on Telegram
2. Copy the bot token
3. Update the token in configuration files:
   - `src/MassageBookingBot.Api/appsettings.json`
   - `src/MassageBookingBot.BotWorker/appsettings.json`

### 2. Build and Run the Backend

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the API
cd src/MassageBookingBot.Api
dotnet run

# Run the Bot Worker (in a new terminal)
cd src/MassageBookingBot.BotWorker
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`)
Swagger UI: `https://localhost:5001/swagger`

### 3. Run the Admin Panel

```bash
cd admin-panel
npm install
npm start
```

The admin panel will be available at `http://localhost:4200`

## Configuration

### Database
- SQLite database is automatically created on first run
- Database file: `massagebooking.db`
- Connection string in `appsettings.json`

### JWT Authentication
- Configure JWT settings in `appsettings.json`
- Default expiry: 24 hours
- Change the secret key for production

### Google Calendar Integration
- Place `credentials.json` in the API project root
- Configure path in `appsettings.json`
- Current implementation uses mock service (implement actual Google Calendar API as needed)

## API Endpoints

### Bookings
- `GET /api/bookings` - Get all bookings (with filters)
- `POST /api/bookings` - Create a new booking
- `DELETE /api/bookings/{id}` - Cancel a booking

### Services
- `GET /api/services` - Get all services

## Telegram Bot Commands

- `/start` - Start the bot and register user
- `/book` - Start booking flow
- `/mybookings` - View your bookings
- `/help` - Show help message

## Database Schema

### Entities
- **User** - Telegram users with referral codes
- **Service** - Available massage services
- **Booking** - Customer bookings
- **TimeSlot** - Available time slots
- **UserState** - FSM state storage

## Scheduled Jobs

### Booking Reminder Job
- Runs every 30 minutes
- Sends 24-hour reminders
- Sends 2-hour reminders
- Configurable via Quartz.NET

## Development

### Adding New Commands/Handlers
1. Add command handler in `BotWorker/Services/BotUpdateHandler.cs`
2. Update FSM states if needed in `Domain/Enums/BotState.cs`
3. Implement business logic using CQRS commands/queries

### Adding New API Endpoints
1. Create command/query in `Application` layer
2. Add controller in `Api/Controllers`
3. API documentation updates automatically via Swagger

## Testing

```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test src/MassageBookingBot.Tests
```

## Deployment

### Docker (Optional)
Create `Dockerfile` for each service and use Docker Compose for orchestration.

### Environment Variables
- `ConnectionStrings__DefaultConnection` - Database connection
- `TelegramBot__Token` - Bot token
- `Jwt__Key` - JWT secret key

## Security Considerations

- Change JWT secret key in production
- Use environment variables for sensitive data
- Implement rate limiting for API endpoints
- Enable HTTPS in production
- Secure database connection strings

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License.

## Support

For issues and questions, please open an issue on GitHub.
