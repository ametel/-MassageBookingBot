using MassageBookingBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MassageBookingBot.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Service> Services { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<TimeSlot> TimeSlots { get; }
    DbSet<UserState> UserStates { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
