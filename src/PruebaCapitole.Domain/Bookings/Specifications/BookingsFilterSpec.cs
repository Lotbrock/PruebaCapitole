using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
using System;
using System.Linq.Expressions;

namespace PruebaCapitole.Domain.Bookings.Specifications
{
    public sealed class BookingsFilterSpec : Specification<Booking>
    {
        private readonly ResourceId? _resourceId;
        private readonly DateTimeOffset? _from;
        private readonly DateTimeOffset? _to;

        public BookingsFilterSpec(ResourceId? resourceId, DateTimeOffset? from, DateTimeOffset? to)
        {
            _resourceId = resourceId;
            _from = from;
            _to = to;
        }

        public override Expression<Func<Booking, bool>> ToExpression()
        {
            var hasResource = _resourceId.HasValue;
            var resource = _resourceId.GetValueOrDefault();
            var hasFrom = _from.HasValue;
            var from = _from.GetValueOrDefault().ToUniversalTime();
            var hasTo = _to.HasValue;
            var to = _to.GetValueOrDefault().ToUniversalTime();

            return b =>
                (!hasResource || b.ResourceId == resource)
                && (!hasTo || b.DateRange.Start < to)    
                && (!hasFrom || b.DateRange.End > from);   
        }
    }
}
