# Secure Token Storage Setup Guide

## Configuration Complete ✅

Your bot token is now stored securely using .NET User Secrets for local development.

## What Was Done

1. **Added to .gitignore**: `appsettings.Development.json` to prevent committing secrets
2. **User Secrets Initialized**: Both API and BotWorker projects now use user secrets
3. **Token Stored Securely**: Token saved in encrypted user secrets store (not in code)
4. **appsettings.json Updated**: Removed hardcoded tokens from configuration files
5. **.env.example Updated**: Template for production deployment

## How It Works

### Local Development (Current Setup)

- Token stored in: `%APPDATA%\Microsoft\UserSecrets\<project-id>\secrets.json` (Windows)
- Automatically loaded by .NET configuration system
- Not tracked in git
- Project-specific and user-specific

### View Your Secrets

```powershell
# BotWorker
cd src/MassageBookingBot.BotWorker
dotnet user-secrets list

# API
cd src/MassageBookingBot.Api
dotnet user-secrets list
```

### Add More Secrets

```powershell
dotnet user-secrets set "Jwt:Key" "YourNewSecureJwtKey32Characters"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your_connection_string"
```

### Remove Secrets

```powershell
dotnet user-secrets remove "TelegramBot:Token"
dotnet user-secrets clear  # Remove all secrets
```

## Production Deployment

### Docker Compose

Use environment variables from `.env` file:

1. Create `.env` file from template:

   ```powershell
   Copy-Item .env.example .env
   ```

2. Edit `.env` with your production values:

   ```
   TELEGRAM_BOT_TOKEN=your_real_token
   JWT_KEY=your_secure_jwt_key_minimum_32_chars
   DB_PASSWORD=your_secure_db_password
   ```

3. Deploy:
   ```powershell
   docker-compose up -d
   ```

### Azure App Service

Use Application Settings (automatically mapped to configuration):

```
TelegramBot__Token = your_token
Jwt__Key = your_jwt_key
```

### AWS / Other Cloud

Use their secrets management:

- AWS: AWS Secrets Manager
- Azure: Azure Key Vault
- GCP: Secret Manager

## Security Best Practices

✅ **Do**:

- Use user secrets for local development
- Use environment variables in Docker
- Use managed secret services in production
- Rotate tokens regularly
- Keep `.env` in `.gitignore`

❌ **Don't**:

- Commit tokens to git
- Share tokens in chat/email
- Use the same token across environments
- Store secrets in appsettings.json

## Current Token Security Status

⚠️ **IMPORTANT**: Your current token was exposed in this conversation.

### Recommended Action:

1. Go to [@BotFather](https://t.me/BotFather) on Telegram
2. Send `/token`
3. Select @SiargukMassageBot
4. Send `/revoke` to get a new token
5. Update the new token:

   ```powershell
   cd src/MassageBookingBot.BotWorker
   dotnet user-secrets set "TelegramBot:Token" "NEW_TOKEN_HERE"

   cd ../MassageBookingBot.Api
   dotnet user-secrets set "TelegramBot:Token" "NEW_TOKEN_HERE"
   ```

## Testing

The bot will now load the token from user secrets automatically. No code changes needed!

```powershell
cd src/MassageBookingBot.BotWorker
dotnet run
```
