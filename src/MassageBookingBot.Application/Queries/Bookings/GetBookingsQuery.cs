using MassageBookingBot.Application.DTOs;
using MassageBookingBot.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MassageBookingBot.Application.Queries.Bookings;

public record GetBookingsQuery(int? UserId = null, DateTime? FromDate = null, DateTime? ToDate = null) : IRequest<List<BookingDto>>;

public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, List<BookingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBookingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookingDto>> Handle(GetBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Service)
            .AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(b => b.UserId == request.UserId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(b => b.BookingDateTime >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(b => b.BookingDateTime <= request.ToDate.Value);

        return await query
            .OrderBy(b => b.BookingDateTime)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                UserName = $"{b.User.FirstName} {b.User.LastName}",
                ServiceId = b.ServiceId,
                ServiceName = b.Service.Name,
                BookingDateTime = b.BookingDateTime,
                Status = b.Status,
                Notes = b.Notes
            })
            .ToListAsync(cancellationToken);
    }
}
