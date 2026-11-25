using System;
using System.Collections.Generic;
using System.Text;

namespace PruebaCapitole.Domain.Bookings
{
    public static class DomainErrors
    {
        public const string Overlap = "booking.overlap";
        public const string InvalidTransition = "booking.invalid_transition";
        public const string StartInPast = "booking.start_in_past";
        public const string InvalidRange = "booking.invalid_range";
        public const string InvalidId = "booking.invalid_id";
        public const string NotFound = "booking.not_found";
        public const string ResourceRequired = "resource.id_required";
        public const string CustomerNameRequired = "customer.name_required";
        public const string CustomerNameTooLong = "customer.name_too_long";
        public const string CustomerEmailInvalid = "customer.email_invalid";
    }
}
