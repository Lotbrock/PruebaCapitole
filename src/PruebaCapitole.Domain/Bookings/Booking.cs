using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaCapitole.Domain.Bookings
{
    public sealed class Booking : AggregateRoot<BookingId>
    {
        public const int ReasonMaxLength = 500;
        public ResourceId ResourceId { get; private set; }
        public Customer Customer { get; private set; }
        public DateRange DateRange { get; private set; }
        public BookingStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? ConfirmedAt { get; private set; }
        public DateTimeOffset? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }

        
        private Booking() { } // EF Core

        public static Booking create(ResourceId resourceId, Customer customer, DateRange dateRange, TimeProvider timeProvider)
        {
            var now = timeProvider.GetUtcNow();

            if (dateRange.Start <= now)
                throw new DomainException(DomainErrors.StartInPast, "La fecha de inicio debe ser futura.");

            var booking = new Booking
            {
                Id = BookingId.New(),
                ResourceId = resourceId,
                Customer = customer,
                DateRange = dateRange,
                Status = BookingStatus.Pending,
                CreatedAt = now
            };

            return booking;
        }


        public void Confirm(bool hasConfirmedOverlap, TimeProvider clock)
        {
            if (Status != BookingStatus.Pending)
                throw InvalidTransition(BookingStatus.Confirmed);

            if (hasConfirmedOverlap)
                throw new DomainException(DomainErrors.Overlap,
                    "Ya existe una reserva confirmada que se solapa para este recurso.");

            var now = clock.GetUtcNow();
            Status = BookingStatus.Confirmed;
            ConfirmedAt = now;

        }

        public void Cancel(string? reason, TimeProvider clock)
        {
            if (Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
                throw InvalidTransition(BookingStatus.Cancelled);

            reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            if (reason is { Length: > ReasonMaxLength })
                reason = reason[..ReasonMaxLength];

            var now = clock.GetUtcNow();
            Status = BookingStatus.Cancelled;
            CancelledAt = now;
            CancellationReason = reason;

        }

        private DomainException InvalidTransition(BookingStatus target) =>
            new(DomainErrors.InvalidTransition, $"No se puede pasar de {Status} a {target}.");


    }
}
