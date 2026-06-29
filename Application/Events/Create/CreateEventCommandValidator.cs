using FluentValidation;

namespace Application.Events.Create
{
    internal sealed class CreateEventCommandValidator
        : AbstractValidator<CreateEventCommand>
    {
        public CreateEventCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(500);

            RuleFor(x => x.VenueId)
                .NotEmpty();

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0);

            RuleFor(x => x.StartDate)
                .GreaterThan(DateTime.UtcNow);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate);

            RuleFor(x => x.TicketPrice)
                .GreaterThan(0);

            RuleFor(x => x.EventType)
                .IsInEnum();
        }
    }
}