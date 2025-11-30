using MassageBookingBot.Application.Interfaces;
using MassageBookingBot.Infrastructure.Jobs;
using MassageBookingBot.Infrastructure.Persistence;
using MassageBookingBot.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace MassageBookingBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=massagebooking.db"));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Services
        services.AddScoped<ICalendarService, GoogleCalendarService>();
        services.AddScoped<INotificationService, TelegramNotificationService>();

        // Quartz
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("BookingReminderJob");
            q.AddJob<BookingReminderJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("BookingReminderJob-trigger")
                .WithCronSchedule("0 */30 * * * ?") // Every 30 minutes
            );
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
