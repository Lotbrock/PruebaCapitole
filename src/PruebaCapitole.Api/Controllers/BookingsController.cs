using MediatR;
using Microsoft.AspNetCore.Mvc;
using PruebaCapitole.Api.Requests;
using PruebaCapitole.Service.Bookings.Commands;
using PruebaCapitole.Service.Bookings.Queries;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Api.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingsController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken ct)
        {
            var id = await sender.Send(new CreateBookingCommand(
                request.ResourceId, request.CustomerName, request.CustomerEmail,
                request.Start, request.End), ct);

            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<BookingListItemDto>>> GetAll(
            [FromQuery] string? resourceId,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? sort,
            CancellationToken ct)
        {
            var result = await sender.Send(
                new GetBookingsQuery(resourceId, from, to, page, pageSize, sort), ct);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookingDetailsDto>> GetById(Guid id, CancellationToken ct)
        {
            var result = await sender.Send(new GetBookingByIdQuery(id), ct);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost("{id:guid}/confirm")]
        public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
        {
            await sender.Send(new ConfirmBookingCommand(id), ct);
            return Ok();
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(
            Guid id, [FromBody] CancelBookingRequest? request, CancellationToken ct)
        {
            await sender.Send(new CancelBookingCommand(id, request?.Reason), ct);
            return Ok();
        }
    }
}
