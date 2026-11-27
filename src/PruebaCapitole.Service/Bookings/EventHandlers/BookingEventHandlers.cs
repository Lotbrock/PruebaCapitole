using MediatR;
using PruebaCapitole.Domain.Bookings.Events;
using PruebaCapitole.Service.Abstractions;
using PruebaCapitole.Shared.IntegrationEvents;

namespace PruebaCapitole.Service.Bookings.EventHandlers
{
    // Domain event -> integration event (public contract) -> Outbox.

    public sealed class BookingConfirmedHandler(IOutbox outbox, ICorrelationIdAccessor correlation)
        : INotificationHandler<DomainEventNotification<BookingConfirmed>>
    {
        public Task Handle(DomainEventNotification<BookingConfirmed> notification, CancellationToken cancellationToken)
        {
            var e = notification.DomainEvent;
            outbox.Add(
                BookingEventsRouting.BookingConfirmedKey,
                new BookingConfirmedV1(
                    Guid.NewGuid(), e.OccurredAt.ToUniversalTime(), correlation.CorrelationId,
                    e.BookingId.Value, e.ResourceId.Value, e.Range.Start, e.Range.End, e.Customer.Email),
                correlation.CorrelationId);
            return Task.CompletedTask;
        }
    }

    public sealed class BookingCancelledHandler(IOutbox outbox, ICorrelationIdAccessor correlation)
        : INotificationHandler<DomainEventNotification<BookingCancelled>>
    {
        public Task Handle(DomainEventNotification<BookingCancelled> notification, CancellationToken cancellationToken)
        {
            var e = notification.DomainEvent;
            outbox.Add(
                BookingEventsRouting.BookingCancelledKey,
                new BookingCancelledV1(
                    Guid.NewGuid(), e.OccurredAt.ToUniversalTime(), correlation.CorrelationId,
                    e.BookingId.Value, e.ResourceId.Value, e.Range.Start, e.Range.End, e.Customer.Email, e.Reason),
                correlation.CorrelationId);
            return Task.CompletedTask;
        }
    }
}
