
using PruebaCapitole.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaCapitole.Domain.Bookings.ValueObjects
{
    public readonly record struct BookingId
    {
        public Guid Value { get; init; }

        public BookingId(Guid value)
        {
            if (value == Guid.Empty)
                throw new DomainException(DomainErrors.InvalidId, "BookingId no puede ser vacío.");
            Value = value;
        }

        public static BookingId New() => new(Guid.NewGuid());

        public override string ToString() => Value.ToString();
    }
}
