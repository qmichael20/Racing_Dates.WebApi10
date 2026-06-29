using FluentValidation;

namespace Application.Reservations.Create
{
    internal sealed class CreateReservationCommandValidator: AbstractValidator<CreateReservationCommand>
    {
        public CreateReservationCommandValidator() 
        {
            RuleFor(x => x.EventId)
                .NotEmpty().WithMessage("EventId is required.");
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
            RuleFor(x => x.BuyerName)
                .NotEmpty().WithMessage("BuyerName is required.");
            RuleFor(x => x.BuyerEmail)
                .NotEmpty().WithMessage("BuyerEmail is required.")
                .EmailAddress().WithMessage("BuyerEmail must be a valid email address.");
        }
    }
}
