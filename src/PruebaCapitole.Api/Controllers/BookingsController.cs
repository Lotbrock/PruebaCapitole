using MediatR;
using Microsoft.AspNetCore.Mvc;
using PruebaCapitole.Api.Extensions;
using PruebaCapitole.Api.Requests;
using PruebaCapitole.Service.Bookings.Commands;
using PruebaCapitole.Service.Bookings.Queries;
using PruebaCapitole.Service.Common;

namespace PruebaCapitole.Api.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public class BookingsController(ISender sender) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken ct)
        {
            var result = await sender.Send(new CreateBookingCommand(
                request.ResourceId, request.CustomerName, request.CustomerEmail,
                request.Start, request.End), ct);

            return this.ToActionResult(result, id =>
                CreatedAtAction(nameof(GetById), new { id }, new { id }));
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<BookingListItemDto>>> GetAll(
            [FromQuery] string? resourceId,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] BookingSortBy? sortBy,
            [FromQuery] SortDirection? sortDirection,
            CancellationToken ct)
        {
            var result = await sender.Send(
                new GetBookingsQuery(resourceId, from, to, page, pageSize, sortBy, sortDirection), ct);

            return this.ToActionResult(result, paged => Ok(paged));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookingDetailsDto>> GetById(Guid id, CancellationToken ct)
        {
            var result = await sender.Send(new GetBookingByIdQuery(id), ct);
            return this.ToActionResult(result, booking => Ok(booking));
        }

        [HttpPost("{id:guid}/confirm")]
        public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
        {
            var result = await sender.Send(new ConfirmBookingCommand(id), ct);
            return this.ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(
            Guid id, [FromBody] CancelBookingRequest? request, CancellationToken ct)
        {
            var result = await sender.Send(new CancelBookingCommand(id, request?.Reason), ct);
            return this.ToActionResult(result);
        }
    }
}
