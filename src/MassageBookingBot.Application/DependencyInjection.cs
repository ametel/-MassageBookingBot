using FluentValidation;
using MassageBookingBot.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MassageBookingBot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add validators
        services.AddValidatorsFromAssemblyContaining<IApplicationDbContext>();
        
        return services;
    }
}
