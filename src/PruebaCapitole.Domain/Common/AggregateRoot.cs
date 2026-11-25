using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaCapitole.Domain.Common
{
    public abstract class AggregateRoot<TId> where TId : notnull
    {
        public TId Id { get; protected set; } = default!;
    }
}
