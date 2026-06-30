using Application.Abstractions.Behaviors;
using Application.Abstractions.Messaging;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using SharedKernel;
using Xunit;

namespace Application.UnitTests.Abstractions.Behaviors
{
    public class ValidatorDecoratorTests
    {
        // Necesitamos comandos y respuestas ficticios para poder instanciar los genéricos del Decorador
        public record TestResponse(string Data);
        public record TestCommand : ICommand<TestResponse>;

        private readonly ICommandHandler<TestCommand, TestResponse> _innerHandler;
        private readonly IValidator<TestCommand> _validator;
        private readonly List<IValidator<TestCommand>> _validators;

        public ValidatorDecoratorTests()
        {
            _innerHandler = Substitute.For<ICommandHandler<TestCommand, TestResponse>>();
            _validator = Substitute.For<IValidator<TestCommand>>();
            _validators = new List<IValidator<TestCommand>> { _validator };
        }

        [Fact]
        public async Task Handle_Should_CallInnerHandler_WhenNoValidatorsAreRegistered()
        {
            // Arrange
            var command = new TestCommand();
            var expectedResponse = Result.Success(new TestResponse("Éxito"));

            // Instanciamos el decorador pasando la lista vacía de validadores
            var decorator = new ValidationDecorator.CommandHandler<TestCommand, TestResponse>(
                _innerHandler,
                new List<IValidator<TestCommand>>());

            _innerHandler.Handle(command, Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await decorator.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedResponse.Value);

            // Verifica que el flujo continuó limpiamente hacia el Handler real
            await _innerHandler.Received(1).Handle(command, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_CallInnerHandler_WhenAllValidatorsPass()
        {
            // Arrange
            var command = new TestCommand();
            var expectedResponse = Result.Success(new TestResponse("Pasó validación"));

            var decorator = new ValidationDecorator.CommandHandler<TestCommand, TestResponse>(_innerHandler, _validators);

            // Simulamos que FluentValidation dice que todo está OK (IsValid = true)
            _validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
                .Returns(new ValidationResult());

            _innerHandler.Handle(command, Arg.Any<CancellationToken>())
                .Returns(expectedResponse);

            // Act
            var result = await decorator.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await _innerHandler.Received(1).Handle(command, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_AndBlockInnerHandler_WhenValidationFails()
        {
            // Arrange
            var command = new TestCommand();
            var decorator = new ValidationDecorator.CommandHandler<TestCommand, TestResponse>(_innerHandler, _validators);

            // Simulamos un fallo de validación inyectando un ValidationFailure estructurado
            var failures = new List<ValidationFailure>
            {
                new("PropiedadEjemplo", "El campo es obligatorio") { ErrorCode = "400" }
            };

            _validator.ValidateAsync(Arg.Any<ValidationContext<TestCommand>>(), Arg.Any<CancellationToken>())
                .Returns(new ValidationResult(failures));

            // Act
            var result = await decorator.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOfType<ValidationError>(); // Valida que retorne tu objeto ValidationError personalizado

            // CRÍTICO: Verificamos que el innerHandler NUNCA se ejecute si hay errores de validación
            await _innerHandler.DidNotReceive().Handle(Arg.Any<TestCommand>(), Arg.Any<CancellationToken>());
        }
    }
}