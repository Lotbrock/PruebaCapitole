using PruebaCapitole.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaCapitole.Domain.Bookings.ValueObjects
{
    public sealed record DateRange
    {
        public DateTimeOffset Start { get; }
        public DateTimeOffset End { get; }

        private DateRange(DateTimeOffset start, DateTimeOffset end)
        {
            Start = start;
            End = end;
        }

        public static DateRange Create(DateTimeOffset start, DateTimeOffset end)
        {
            if (start >= end)
                throw new DomainException(DomainErrors.InvalidRange, "Start debe ser anterior a End.");

            return new DateRange(start.ToUniversalTime(), end.ToUniversalTime());
        }

        public bool Overlaps(DateRange other) => Start < other.End && other.Start < End;


    }
}
