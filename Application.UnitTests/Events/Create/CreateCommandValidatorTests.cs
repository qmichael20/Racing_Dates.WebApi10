using Application.Events.Create;
using Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.UnitTests.Events.Create
{
    public class CreateCommandValidatorTests
    {
        private readonly CreateEventCommandValidator _validator;

        public CreateCommandValidatorTests()
        {
            _validator = new CreateEventCommandValidator();
        }

        private CreateEventCommand CreateDefaultCommand(
            string title = "Concierto de Rock",
            string description = "Un evento increíble de música en vivo",
            Guid? venueId = null,
            int maxCapacity = 100,
            DateTime? startDate = null,
            DateTime? endDate = null,
            decimal ticketPrice = 45.50m,
            EventType eventType = EventType.Conference)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(1);
            return new CreateEventCommand(
                Title: title,
                Description: description,
                VenueId: venueId ?? Guid.NewGuid(),
                MaxCapacity: maxCapacity,
                StartDate: start,
                EndDate: endDate ?? start.AddHours(2),
                TicketPrice: ticketPrice,
                EventType: eventType
            );
        }

        [Fact]
        public void Validator_Should_NotHaveAnyErrors_WhenCommandIsValid()
        {
            // Arrange
            var command = CreateDefaultCommand();

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
        
        [Theory]
        [InlineData("")]
        [InlineData("Abc")] // Menor a 5 caracteres
        public void Validator_Should_HaveError_WhenTitleIsInvalid(string invalidTitle)
        {
            // Arrange
            var command = CreateDefaultCommand(title: invalidTitle);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Validator_Should_HaveError_WhenDescriptionIsTooShort()
        {
            // Arrange
            var command = CreateDefaultCommand(description: "Corto"); // Menor a 10 caracteres

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validator_Should_HaveError_WhenVenueIdIsEmpty()
        {
            // Arrange
            var command = CreateDefaultCommand(venueId: Guid.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.VenueId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Validator_Should_HaveError_WhenMaxCapacityIsLessThanOrEqualToZero(int invalidCapacity)
        {
            // Arrange
            var command = CreateDefaultCommand(maxCapacity: invalidCapacity);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MaxCapacity);
        }

        [Fact]
        public void Validator_Should_HaveError_WhenStartDateIsInThePast()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddHours(-1);
            var command = CreateDefaultCommand(startDate: pastDate, endDate: pastDate.AddHours(2));

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StartDate);
        }

        [Fact]
        public void Validator_Should_HaveError_WhenEndDateIsBeforeStartDate()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(1);
            var invalidEndDate = startDate.AddHours(-2); // Termina antes de empezar
            var command = CreateDefaultCommand(startDate: startDate, endDate: invalidEndDate);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EndDate);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5.50)]
        public void Validator_Should_HaveError_WhenTicketPriceIsLessThanOrEqualToZero(decimal invalidPrice)
        {
            // Arrange
            var command = CreateDefaultCommand(ticketPrice: invalidPrice);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TicketPrice);
        }

        [Fact]
        public void Validator_Should_HaveError_WhenEventTypeIsInvalidEnum()
        {
            // Arrange
            var command = CreateDefaultCommand(eventType: (EventType)999); // Casteo a un enum que no existe

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EventType);
        }
    }
}