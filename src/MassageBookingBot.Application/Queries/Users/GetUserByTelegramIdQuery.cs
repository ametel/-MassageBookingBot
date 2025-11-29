using MassageBookingBot.Application.DTOs;
using MassageBookingBot.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MassageBookingBot.Application.Queries.Users;

public record GetUserByTelegramIdQuery(long TelegramUserId) : IRequest<UserDto?>;

public class GetUserByTelegramIdQueryHandler : IRequestHandler<GetUserByTelegramIdQuery, UserDto?>
{
    private readonly IApplicationDbContext _context;

    public GetUserByTelegramIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> Handle(GetUserByTelegramIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.TelegramUserId == request.TelegramUserId, cancellationToken);

        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            TelegramUserId = user.TelegramUserId,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ReferralCode = user.ReferralCode,
            ReferralCount = user.ReferralCount,
            DiscountBalance = user.DiscountBalance
        };
    }
}
