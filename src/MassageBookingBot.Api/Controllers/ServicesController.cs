using MassageBookingBot.Application.DTOs;
using MassageBookingBot.Application.Queries.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MassageBookingBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceDto>>> GetServices([FromQuery] bool? activeOnly = true)
    {
        var services = await _mediator.Send(new GetServicesQuery(activeOnly));
        return Ok(services);
    }
}
