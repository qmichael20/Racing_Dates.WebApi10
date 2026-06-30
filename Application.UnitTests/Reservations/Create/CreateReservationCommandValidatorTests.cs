using Application.Reservations.Create;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.UnitTests.Reservations.Create
{
    public class CreateReservationCommandValidatorTests
    {
        private readonly CreateReservationCommandValidator _validator;

        public CreateReservationCommandValidatorTests()
        {
            _validator = new CreateReservationCommandValidator();
        }

        private CreateReservationCommand CreateDefaultCommand(
            Guid? eventId = null,
            int quantity = 2,
            string buyerName = "Eduardo Mendoza",
            string buyerEmail = "eduardo.mendoza@mail.com")
        {
            return new CreateReservationCommand(
                EventId: eventId ?? Guid.NewGuid(),
                Quantity: quantity,
                BuyerName: buyerName,
                BuyerEmail: buyerEmail
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

        [Fact]
        public void Validator_Should_HaveError_WhenEventIdIsEmpty()
        {
            // Arrange
            var command = CreateDefaultCommand(eventId: Guid.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EventId)
                .WithErrorMessage("EventId is required.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Validator_Should_HaveError_WhenQuantityIsLessThanOrEqualToZero(int invalidQuantity)
        {
            // Arrange
            var command = CreateDefaultCommand(quantity: invalidQuantity);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantity)
                .WithErrorMessage("Quantity must be greater than 0.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validator_Should_HaveError_WhenBuyerNameIsEmpty(string invalidName)
        {
            // Arrange
            var command = CreateDefaultCommand(buyerName: invalidName);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BuyerName)
                .WithErrorMessage("BuyerName is required.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validator_Should_HaveError_WhenBuyerEmailIsEmpty(string invalidEmail)
        {
            // Arrange
            var command = CreateDefaultCommand(buyerEmail: invalidEmail);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BuyerEmail)
                .WithErrorMessage("BuyerEmail is required.");
        }

        [Theory]
        [InlineData("correoInvalido")]
        [InlineData("usuario@")]
        [InlineData("@dominio.com")]
        public void Validator_Should_HaveError_WhenBuyerEmailIsInvalidFormat(string invalidEmail)
        {
            // Arrange
            var command = CreateDefaultCommand(buyerEmail: invalidEmail);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.BuyerEmail)
                .WithErrorMessage("BuyerEmail must be a valid email address.");
        }
    }
}