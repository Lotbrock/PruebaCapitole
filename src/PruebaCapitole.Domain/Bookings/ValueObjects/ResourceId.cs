using PruebaCapitole.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaCapitole.Domain.Bookings.ValueObjects
{
    public readonly record struct ResourceId
    {
        public string Value { get; init; }
        public ResourceId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException(DomainErrors.ResourceRequired, "ResourceId es obligatorio.");
            Value = value.Trim();
        }

        public override string ToString() => Value;
    }
}
