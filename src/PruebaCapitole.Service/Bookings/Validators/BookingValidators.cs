using FluentValidation;
using PruebaCapitole.Domain.Bookings;
using PruebaCapitole.Domain.Bookings.ValueObjects;
using PruebaCapitole.Service.Bookings.Commands;
using PruebaCapitole.Service.Bookings.Queries;

namespace PruebaCapitole.Service.Bookings.Validators
{
    // Format/shape checks (-> 400). Business rules (overlap, past start...) stay in the Domain.

    public sealed class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingValidator(TimeProvider clock)
        {
            RuleFor(x => x.ResourceId).NotEmpty().MaximumLength(100);
            RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(Customer.NameMaxLength);
            RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(320);
            RuleFor(x => x.Start).Must(start => start > clock.GetUtcNow())
                .WithMessage("'Start' debe ser una fecha futura.");
            RuleFor(x => x.End).GreaterThan(x => x.Start)
                .WithMessage("'End' debe ser posterior a 'Start'.");
        }
    }

    public sealed class ConfirmBookingValidator : AbstractValidator<ConfirmBookingCommand>
    {
        public ConfirmBookingValidator() => RuleFor(x => x.BookingId).NotEmpty();
    }

    public sealed class CancelBookingValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingValidator()
        {
            RuleFor(x => x.BookingId).NotEmpty();
            RuleFor(x => x.Reason).MaximumLength(Booking.ReasonMaxLength);
        }
    }

    public sealed class GetBookingsValidator : AbstractValidator<GetBookingsQuery>
    {
        public GetBookingsValidator()
        {
            RuleFor(x => x.To).GreaterThan(x => x.From)
                .When(x => x.From.HasValue && x.To.HasValue)
                .WithMessage("'from' debe ser anterior a 'to'.");
        }
    }
}
