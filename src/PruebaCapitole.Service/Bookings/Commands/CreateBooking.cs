using MediatR;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Service.Abstractions;

namespace PruebaCapitole.Service.Bookings.Commands
{
    public sealed record CreateBookingCommand(
        string ResourceId,
        string CustomerName,
        string CustomerEmail,
        DateTimeOffset Start,
        DateTimeOffset End) : IRequest<Guid>;

    public sealed class CreateBookingHandler(
        IBookingRepository repository,
        IUnitOfWork unitOfWork,
        TimeProvider clock) : IRequestHandler<CreateBookingCommand, Guid>
    {
        public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
             var resourceId = new ResourceId(request.ResourceId);
            var customer = Customer.Create(request.CustomerName, request.CustomerEmail);
            var dateRange = DateRange.Create(request.Start, request.End);

           var booking = Booking.create(resourceId, customer, dateRange, clock);

            await repository.AddAsync(booking, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return booking.Id.Value;
        }
    }
}
