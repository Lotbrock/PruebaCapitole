using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PruebaCapitole.Domain.Bookings.Specifications
{
    public sealed class ConfirmedOverlappingBookingSpec : Specification<Booking>
    {
        private readonly ResourceId _resourceId;
        private readonly DateRange _range;
        private readonly BookingId? _excludeId;
        public ConfirmedOverlappingBookingSpec(ResourceId resourceId, DateRange range, BookingId? excludeId = null)
        {
            _resourceId = resourceId;
            _range = range;
            _excludeId = excludeId;
        }
        public override Expression<Func<Booking, bool>> ToExpression()
        {
            var resource = _resourceId;
            var start = _range.Start;
            var end = _range.End;
            var hasExclude = _excludeId.HasValue;
            var excluded = _excludeId.GetValueOrDefault();
            return b =>
                b.Status == BookingStatus.Confirmed
                && b.ResourceId == resource
                && (!hasExclude || b.Id != excluded)
                && b.DateRange.Start < end
                && start < b.DateRange.End;
        }
    }
}
