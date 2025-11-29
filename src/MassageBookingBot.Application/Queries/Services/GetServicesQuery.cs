using MassageBookingBot.Application.DTOs;
using MassageBookingBot.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MassageBookingBot.Application.Queries.Services;

public record GetServicesQuery(bool? ActiveOnly = true) : IRequest<List<ServiceDto>>;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, List<ServiceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceDto>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Services.AsQueryable();

        if (request.ActiveOnly == true)
            query = query.Where(s => s.IsActive);

        return await query
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                IsActive = s.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
