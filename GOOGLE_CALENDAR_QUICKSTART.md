# Google Calendar Quick Start

## Prerequisites
- Google account
- Access to Google Cloud Console
- Massage Booking Bot repository

## Setup Steps

### 1. Google Cloud Setup (5 minutes)

1. **Create/Select Project**
   - Visit: https://console.cloud.google.com/
   - Create new project or select existing

2. **Enable Calendar API**
   ```
   APIs & Services → Library → Search "Google Calendar API" → Enable
   ```

3. **Create Service Account**
   ```
   APIs & Services → Credentials → Create Credentials → Service Account
   Name: massage-bot-calendar
   Skip role assignment → Done
   ```

4. **Generate Key**
   ```
   Click on service account → Keys tab → Add Key → Create New Key → JSON
   Download and save as: google-service-account.json
   ```

5. **Copy Service Account Email**
   ```
   From JSON file, copy the "client_email" value
   Example: massage-bot-calendar@your-project.iam.gserviceaccount.com
   ```

### 2. Google Calendar Setup (2 minutes)

1. Open Google Calendar (calendar.google.com)
2. Select calendar or create new one (e.g., "Massage Appointments")
3. Click ⋮ next to calendar name → Settings and sharing
4. Under "Share with specific people":
   - Click "Add people"
   - Paste service account email
   - Permission: "Make changes to events"
   - Click Send

5. Get Calendar ID:
   - In calendar settings, scroll to "Integrate calendar"
   - Copy "Calendar ID" (looks like: abc123@group.calendar.google.com)

### 3. Application Configuration (3 minutes)

1. **Place Service Account Key**
   ```bash
   # Copy to API project folder
   cp google-service-account.json src/MassageBookingBot.Api/
   ```

2. **Update appsettings.json**
   ```json
   {
     "GoogleCalendar": {
       "ServiceAccountKeyPath": "google-service-account.json",
       "CalendarId": "abc123@group.calendar.google.com",
       "TimeZone": "America/New_York",
       "ApplicationName": "Massage Booking Bot"
     }
   }
   ```

3. **Update appsettings.Development.json** (same as above)

4. **Update BotWorker appsettings**
   ```bash
   # Edit: src/MassageBookingBot.BotWorker/appsettings.json
   ```
   ```json
   {
     "GoogleCalendar": {
       "ServiceAccountKeyPath": "../MassageBookingBot.Api/google-service-account.json",
       "CalendarId": "abc123@group.calendar.google.com",
       "TimeZone": "America/New_York",
       "ApplicationName": "Massage Booking Bot"
     }
   }
   ```

### 4. Test Integration (2 minutes)

1. **Build & Run**
   ```bash
   dotnet build
   dotnet run --project src/MassageBookingBot.Api
   ```

2. **Create Test Booking**
   ```bash
   curl -X POST http://localhost:5000/api/bookings \
     -H "Content-Type: application/json" \
     -d '{
       "userId": 1,
       "serviceId": 1,
       "bookingDateTime": "2025-12-01T14:00:00",
       "notes": "Test booking"
     }'
   ```

3. **Verify in Google Calendar**
   - Open Google Calendar
   - Check for new event on Dec 1, 2025 at 2:00 PM
   - Event should include: service name, client info

## Troubleshooting

### "Service account key file not found"
✓ Check path in appsettings.json
✓ Ensure file exists in correct location
✓ Use absolute path if needed

### "Failed to initialize Google Calendar service"
✓ Verify JSON file is valid
✓ Check Google Calendar API is enabled
✓ Confirm service account exists in Cloud Console

### "Failed to create calendar event"
✓ Verify calendar is shared with service account
✓ Check permission is "Make changes to events"
✓ Confirm CalendarId is correct

### "404 Not Found" on event operations
✓ Event may have been manually deleted
✓ Wrong calendar ID
✓ Calendar may have been unshared

## Common TimeZones

- US Eastern: `America/New_York`
- US Pacific: `America/Los_Angeles`
- UK: `Europe/London`
- EU Central: `Europe/Paris`
- Australia: `Australia/Sydney`
- Asia: `Asia/Tokyo`, `Asia/Singapore`

Full list: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones

## Security Checklist

- [ ] Service account JSON NOT committed to git
- [ ] `.gitignore` includes `google-service-account.json`
- [ ] File permissions restricted (chmod 600)
- [ ] Separate service accounts for dev/staging/production
- [ ] Calendar access limited to specific service account only
- [ ] Regular key rotation scheduled

## Production Deployment

For production environments:

1. **Use Secret Management**
   - Azure: Key Vault
   - AWS: Secrets Manager
   - GCP: Secret Manager
   - Kubernetes: Sealed Secrets

2. **Environment Variables**
   ```bash
   export GOOGLE_APPLICATION_CREDENTIALS=/path/to/key.json
   ```

3. **Docker**
   ```dockerfile
   # Mount as secret
   COPY google-service-account.json /app/secrets/
   ENV GoogleCalendar__ServiceAccountKeyPath=/app/secrets/google-service-account.json
   ```

## Next Steps

After successful setup:

1. Test all CRUD operations (Create, Update, Delete bookings)
2. Monitor logs for calendar sync issues
3. Set up error alerting
4. Configure reminder preferences
5. Customize event descriptions/formatting

## Support

For detailed documentation, see: [GOOGLE_CALENDAR_INTEGRATION.md](./GOOGLE_CALENDAR_INTEGRATION.md)

For issues:
- Check application logs: `logs/api-*.log`
- Review Google Cloud Console audit logs
- Verify API quota usage (should be well within free tier)
