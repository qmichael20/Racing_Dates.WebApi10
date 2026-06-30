using Application.Ports;
using Application.Reservations.Confirm;
using Domain.Enums;
using Domain.Reservations;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using System.Reflection;
using Xunit;
using NSubstitute.Extensions;

namespace Application.UnitTests.Reservations.Confirm
{
    public class ConfirmReservationCommandHandlerTests
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ConfirmReservationCommandHandler _handler;

        public ConfirmReservationCommandHandlerTests()
        {
            _reservationRepository = Substitute.For<IReservationRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _handler = new ConfirmReservationCommandHandler(_reservationRepository, _unitOfWork);
        }

        private ConfirmReservationCommand CreateDefaultCommand()
        {
            return new ConfirmReservationCommand(ReservationId: Guid.NewGuid());
        }

        private Reservation CreateDefaultReservation(ReservationStatus status = ReservationStatus.Pending)
        {
            var reservation = new Reservation(
                id: Guid.NewGuid(),
                eventId: Guid.NewGuid(),
                quantity: 2,
                buyerName: "Alberto Mezas",
                buyerEmail: "mezas.pelucas@mail.com"
            );

            // Forzamos el Status interno usando Reflexión por si el setter es privado/init
            typeof(Reservation)
                .GetProperty(nameof(Reservation.Status))?
                .SetValue(reservation, status);

            return reservation;
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenReservationDoesNotExist()
        {
            // Arrange
            var command = CreateDefaultCommand();
            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns((Reservation?)null);

            // Act
            Result<string> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.NotFound(command.ReservationId));
            await _unitOfWork.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenReservationIsAlreadyConfirmed()
        {
            // Arrange
            var command = CreateDefaultCommand();
            var confirmedReservation = CreateDefaultReservation(status: ReservationStatus.Confirmed);

            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns(confirmedReservation);

            // Act
            Result<string> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.AlreadyConfirmed);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenReservationIsAlreadyCancelled()
        {
            // Arrange
            var command = CreateDefaultCommand();
            var cancelledReservation = CreateDefaultReservation(status: ReservationStatus.Cancelled);

            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns(cancelledReservation);

            // Act
            Result<string> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.AlreadyCancelled);
        }

        [Fact]
        public async Task Handle_Should_LoopAndGenerateNewCode_WhenGeneratedCodeAlreadyExists()
        {
            // Arrange
            var command = CreateDefaultCommand();
            var reservation = CreateDefaultReservation(status: ReservationStatus.Pending);

            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns(reservation);

            // CORRECCIÓN AQUÍ: Usamos .Returns con Task.FromResult secuenciales
            _reservationRepository.CodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(true), Task.FromResult(false));

            // Act
            Result<string> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().StartWith("EV-");

            await _reservationRepository.Received(2).CodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnValidCodeAndConfirm_WhenReservationIsPendingAndCodeIsUnique()
        {
            // Arrange
            var command = CreateDefaultCommand();
            var reservation = CreateDefaultReservation(status: ReservationStatus.Pending);

            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns(reservation);

            _reservationRepository.CodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(false); // El código no existe, pasa directo

            // Act
            Result<string> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().MatchRegex(@"^EV-\d{6}$"); // Verifica el patrón exacto EV-XXXXXX (6 dígitos)

            // Validar que el estado de la entidad mutó a Confirmed
            reservation.Status.Should().Be(ReservationStatus.Confirmed);

            await _unitOfWork.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }
    }
}