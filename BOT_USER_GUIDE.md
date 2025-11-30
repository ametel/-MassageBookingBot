# 🤖 Telegram Bot User Guide - Massage Booking Bot

## Getting Started with Your Massage Booking Bot

This guide will help you set up and start using your Telegram massage booking bot in just a few minutes!

---

## 📋 Prerequisites

Before you can use the bot, you need:

- A Telegram account
- The backend API server running (see below)
- A bot token from Telegram

---

## 🚀 Quick Start (5 Minutes)

### Step 1: Create Your Bot on Telegram

1. **Open Telegram** on your phone or desktop
2. **Search for** `@BotFather` (official Telegram bot for creating bots)
3. **Start a chat** with BotFather
4. **Send the command**: `/newbot`
5. **Follow the prompts**:
   - Choose a **name** for your bot (e.g., "My Massage Booking Bot")
   - Choose a **username** for your bot (must end with "bot", e.g., "mymassagebooking_bot")
6. **Copy the token** that BotFather gives you (it looks like: `123456789:ABCdefGHIjklMNOpqrsTUVwxyz`)

**Example conversation:**

```
You: /newbot
BotFather: Alright, a new bot. How are we going to call it?
You: My Massage Booking Bot
BotFather: Good. Now let's choose a username for your bot.
You: mymassagebooking_bot
BotFather: Done! Congratulations on your new bot. Here is your token:
           123456789:ABCdefGHIjklMNOpqrsTUVwxyz
```

⚠️ **Keep this token secret!** Anyone with this token can control your bot.

---

### Step 2: Configure Your Bot Token

You need to add the token to two configuration files:

#### File 1: API Configuration

**Location**: `src/MassageBookingBot.Api/appsettings.json`

Find this section:

```json
{
  "TelegramBot": {
    "Token": ""
  }
}
```

Replace with your token:

```json
{
  "TelegramBot": {
    "Token": "123456789:ABCdefGHIjklMNOpqrsTUVwxyz"
  }
}
```

#### File 2: Bot Worker Configuration

**Location**: `src/MassageBookingBot.BotWorker/appsettings.json`

Find this section:

```json
{
  "TelegramBot": {
    "Token": ""
  }
}
```

Replace with your token (same as above):

```json
{
  "TelegramBot": {
    "Token": "123456789:ABCdefGHIjklMNOpqrsTUVwxyz"
  }
}
```

---

### Step 3: Start the Services

Open **two separate PowerShell terminals**:

#### Terminal 1: Start the API Server

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.Api
dotnet run
```

**Wait for**: `Now listening on: http://localhost:5000`

#### Terminal 2: Start the Bot Worker

```powershell
cd K:\SmallProjects\BOT\-MassageBookingBot\src\MassageBookingBot.BotWorker
dotnet run
```

**Wait for**: Messages indicating the bot is polling for updates

✅ **Your bot is now live!**

---

## 💬 Using the Bot

### Step 4: Find Your Bot on Telegram

1. Open Telegram
2. Search for your bot's username (e.g., `@mymassagebooking_bot`)
3. Click on it to open the chat
4. Click **"START"** button or send `/start`

---

## 🎯 Available Commands

### `/start` - Register & Get Your Referral Code

**What it does:**

- Registers you as a new user
- Generates a unique referral code for you
- Shows welcome message

**Example:**

```
You: /start

Bot: 👋 Welcome to Massage Booking Bot!

You've been successfully registered.
Your unique referral code: ABC123XYZ

Share this code with friends to earn discounts!

Use /book to make a booking
Use /mybookings to view your appointments
Use /help for more information
```

---

### `/book` - Book a Massage Appointment

**What it does:**

- Starts the interactive booking process
- Guides you through selecting service, date, and time

**Booking Flow:**

#### Step 1: Choose a Service

```
Bot: 💆 Please select a massage service:

[Swedish Massage - $80 (60 min)]
[Deep Tissue - $100 (75 min)]
[Hot Stone - $120 (90 min)]
[Sports Massage - $90 (60 min)]
[Aromatherapy - $110 (75 min)]
```

**Click** on the service you want.

#### Step 2: Choose a Date

```
Bot: 📅 Select a date:

[Today - Dec 1]
[Tomorrow - Dec 2]
[Dec 3, 2025]
[Dec 4, 2025]
[Dec 5, 2025]
[Dec 6, 2025]
[Dec 7, 2025]
```

**Click** on your preferred date.

#### Step 3: Choose a Time

```
Bot: ⏰ Available times for Dec 1:

[09:00 AM]
[10:00 AM]
[11:00 AM]
[12:00 PM]
[01:00 PM]
[02:00 PM]
[03:00 PM]
[04:00 PM]
```

**Click** on your preferred time slot.

#### Step 4: Confirm

```
Bot: ✅ Booking Summary:

Service: Swedish Massage ($80)
Date: December 1, 2025
Time: 10:00 AM
Duration: 60 minutes

Please confirm your booking:

[✅ Confirm] [❌ Cancel]
```

**Click** "Confirm" to complete the booking.

#### Step 5: Confirmation

```
Bot: 🎉 Booking Confirmed!

Your appointment has been successfully booked:

📍 Swedish Massage
📅 December 1, 2025
⏰ 10:00 AM
⏱️ 60 minutes

You will receive:
- A reminder 24 hours before your appointment
- A reminder 2 hours before your appointment

See you soon! 💆‍♀️
```

---

### `/mybookings` - View Your Appointments

**What it does:**

- Shows all your active bookings
- Displays upcoming appointments

**Example:**

```
You: /mybookings

Bot: 📋 Your Bookings:

1. Swedish Massage
   📅 Dec 1, 2025 at 10:00 AM
   ⏱️ 60 minutes | 💰 $80
   Status: Confirmed ✅

2. Deep Tissue Massage
   📅 Dec 5, 2025 at 02:00 PM
   ⏱️ 75 minutes | 💰 $100
   Status: Confirmed ✅

Total bookings: 2
```

If you have no bookings:

```
Bot: 📋 You have no active bookings.

Use /book to make a new booking! 💆
```

---

### `/help` - Get Help

**What it does:**

- Shows list of available commands
- Explains bot features

**Example:**

```
You: /help

Bot: 📖 Help & Commands

Available commands:
/start - Register and get your referral code
/book - Book a new massage appointment
/mybookings - View your active bookings
/help - Show this help message

Features:
✅ Easy booking process
✅ Multiple massage services
✅ Automated reminders
✅ Referral system with discounts

Need assistance? Contact support at: [your email]
```

---

## 🎁 Referral System

### How It Works

1. **Get Your Code**: After `/start`, you receive a unique code (e.g., "ABC123XYZ")
2. **Share It**: Give your code to friends
3. **They Register**: Friend sends `/start ABC123XYZ` (with your code)
4. **You Earn**: You get referral points and discounts!

### Using a Referral Code

If someone gave you a referral code:

```
/start THEIR_REFERRAL_CODE
```

**Example:**

```
You: /start ABC123XYZ

Bot: 👋 Welcome to Massage Booking Bot!

You've been registered using referral code: ABC123XYZ
The referrer will receive bonus points!

Your unique referral code: DEF456UVW

Use /book to make a booking
```

---

## 📬 Automatic Notifications

Your bot will automatically send you reminders:

### Immediate Confirmation

Right after booking:

```
Bot: ✅ Booking Confirmation

Your appointment is confirmed for:
📅 December 1, 2025 at 10:00 AM
💆 Swedish Massage

Save this date! See you there! 🎉
```

### 24-Hour Reminder

One day before your appointment:

```
Bot: ⏰ Reminder: 24 Hours

Don't forget your appointment tomorrow!

📅 December 1, 2025 at 10:00 AM
💆 Swedish Massage
⏱️ 60 minutes

See you soon! 💆‍♀️
```

### 2-Hour Reminder

Two hours before your appointment:

```
Bot: ⏰ Reminder: 2 Hours

Your appointment is in 2 hours!

📅 Today at 10:00 AM
💆 Swedish Massage

We're looking forward to seeing you! 🌟
```

---

## 🔄 Booking States

The bot remembers where you are in the booking process:

- **Idle**: No active booking process
- **Selecting Service**: Waiting for you to choose a service
- **Selecting Date**: Waiting for you to choose a date
- **Selecting Time**: Waiting for you to choose a time slot
- **Confirming**: Waiting for you to confirm the booking

💡 **Tip**: You can restart the process anytime by sending `/start`

---

## ❓ Common Questions

### Q: Can I book multiple appointments?

**A:** Yes! Use `/book` as many times as you want. Each booking is tracked separately.

### Q: How do I cancel a booking?

**A:** Currently, cancellations must be done through the admin panel or by contacting support. (Future feature: cancel via bot)

### Q: What if my preferred time is not available?

**A:** The bot only shows available time slots. If you don't see your preferred time, try a different date.

### Q: Do I need to pay through the bot?

**A:** Currently, payment is handled separately. The bot is for booking only. (Future feature: payment integration)

### Q: Can I reschedule an appointment?

**A:** Currently, you need to cancel the old booking and create a new one. (Future feature: direct rescheduling)

### Q: What services are available?

**A:**

- Swedish Massage - $80 (60 min)
- Deep Tissue Massage - $100 (75 min)
- Hot Stone Massage - $120 (90 min)
- Sports Massage - $90 (60 min)
- Aromatherapy Massage - $110 (75 min)

### Q: What are the operating hours?

**A:** Available booking slots are from 9:00 AM to 5:00 PM daily.

---

## 🛠️ Troubleshooting

### Bot doesn't respond

**Check:**

1. Is the API server running? (Terminal 1)
2. Is the Bot Worker running? (Terminal 2)
3. Is the bot token correctly configured?
4. Try sending `/start` again

### "Bot not found" in Telegram search

**Solution:**

- Double-check the bot username
- Make sure you completed Step 1 with BotFather
- Try searching with the @ symbol (e.g., `@mymassagebooking_bot`)

### Booking process stuck

**Solution:**

1. Send `/start` to reset your state
2. Try the `/book` command again
3. Check that the API server is running

### Not receiving reminders

**Check:**

1. Quartz.NET scheduler is running (check API server logs)
2. Your booking is confirmed (use `/mybookings`)
3. Wait for the next scheduler run (every 30 minutes)

---

## 🔐 Privacy & Security

- Your Telegram user data is stored securely in the database
- Your phone number is only collected if you provide it
- Bot token should never be shared
- All communication is through Telegram's secure platform

---

## 📞 Support

If you encounter issues:

1. **Check Logs**: Look at the console output in both terminals
2. **Restart Services**: Stop (Ctrl+C) and restart both API and Bot Worker
3. **Check Configuration**: Verify the bot token is correctly set
4. **Review Documentation**: See `README.md` and `TESTING_PLAN.md`

---

## 🎉 You're Ready!

Your bot is now configured and ready to use. Simply:

1. ✅ Open Telegram
2. ✅ Find your bot
3. ✅ Send `/start`
4. ✅ Start booking!

Enjoy your automated massage booking system! 💆‍♀️💆‍♂️

---

## 📚 Additional Resources

- **Full Setup Guide**: `SETUP.md`
- **Testing Guide**: `TESTING_PLAN.md`
- **Quick Reference**: `RUNBOOK.md`
- **Technical Details**: `IMPLEMENTATION_SUMMARY.md`

---

_Last Updated: November 30, 2025_
_Need help? Check the troubleshooting section or review the logs in your terminal._
