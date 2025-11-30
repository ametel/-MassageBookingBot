using MassageBookingBot.Domain.Entities;

namespace MassageBookingBot.Infrastructure.Persistence;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        // Check if database is already seeded
        if (context.Services.Any())
        {
            return;
        }

        // Seed Services
        var services = new[]
        {
            new Service
            {
                Name = "Swedish Massage",
                Description = "Classic relaxation massage with gentle pressure",
                Price = 80,
                DurationMinutes = 60,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                Name = "Deep Tissue Massage",
                Description = "Intense massage targeting deep muscle layers",
                Price = 100,
                DurationMinutes = 60,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                Name = "Hot Stone Massage",
                Description = "Relaxing massage using heated stones",
                Price = 120,
                DurationMinutes = 90,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                Name = "Sports Massage",
                Description = "Therapeutic massage for athletes and active individuals",
                Price = 90,
                DurationMinutes = 60,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Service
            {
                Name = "Aromatherapy Massage",
                Description = "Gentle massage with essential oils",
                Price = 110,
                DurationMinutes = 75,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Services.AddRange(services);
        context.SaveChanges();

        // Seed TimeSlots for the next 7 days
        var now = DateTime.Today;
        var timeSlots = new List<TimeSlot>();

        for (int day = 1; day <= 7; day++)
        {
            var date = now.AddDays(day);
            
            // Add slots from 9 AM to 5 PM (every hour)
            for (int hour = 9; hour < 17; hour++)
            {
                timeSlots.Add(new TimeSlot
                {
                    StartTime = date.AddHours(hour),
                    EndTime = date.AddHours(hour + 1),
                    IsAvailable = true,
                    IsBooked = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        context.TimeSlots.AddRange(timeSlots);
        context.SaveChanges();
    }
}
