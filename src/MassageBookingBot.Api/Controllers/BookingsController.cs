using MassageBookingBot.Application.Commands.Bookings;
using MassageBookingBot.Application.DTOs;
using MassageBookingBot.Application.Queries.Bookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBookingBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookingDto>>> GetBookings(
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var bookings = await _mediator.Send(new GetBookingsQuery(userId, fromDate, toDate));
        return Ok(bookings);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateBooking([FromBody] CreateBookingDto booking)
    {
        var bookingId = await _mediator.Send(new CreateBookingCommand(booking));
        return CreatedAtAction(nameof(GetBookings), new { id = bookingId }, bookingId);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> CancelBooking(int id)
    {
        var result = await _mediator.Send(new CancelBookingCommand(id));
        if (!result)
            return NotFound();
        return NoContent();
    }
}
