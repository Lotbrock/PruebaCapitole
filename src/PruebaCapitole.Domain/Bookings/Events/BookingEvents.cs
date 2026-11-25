
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaCapitole.Domain.Bookings.Events
{// Eventos de DOMINIO: internos. Los eventos de integración (proyecto Shared) son otra cosa.
    public sealed record BookingCreated(
        BookingId BookingId, ResourceId ResourceId, DateRange Range, Customer Customer,
        DateTimeOffset OccurredAt) : IDomainEvent;

    public sealed record BookingConfirmed(
        BookingId BookingId, ResourceId ResourceId, DateRange Range, Customer Customer,
        DateTimeOffset OccurredAt) : IDomainEvent;

    public sealed record BookingCancelled(
        BookingId BookingId, ResourceId ResourceId, DateRange Range, Customer Customer,
        string? Reason, DateTimeOffset OccurredAt) : IDomainEvent;

}
