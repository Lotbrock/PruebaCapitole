using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PruebaCapitole.Domain.Common
{
    public interface IDomainEvent
    {
        DateTimeOffset OccurredAt { get; }

    }
}
