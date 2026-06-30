using Application.Ports;
using Application.Reservations.Cancel;
using Domain.Enums;
using Domain.Events;
using Domain.Reservations;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using System.Reflection;
using Xunit;

namespace Application.UnitTests.Reservations.Cancel
{
    public class CancelReservationCommandHandlerTests
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CancekReservationCommandHandler _handler;

        public CancelReservationCommandHandlerTests()
        {
            _reservationRepository = Substitute.For<IReservationRepository>();
            _eventRepository = Substitute.For<IEventRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _handler = new CancekReservationCommandHandler(
                _reservationRepository,
                _eventRepository,
                _unitOfWork);
        }

        private CancelReservationCommand CreateDefaultCommand()
        {
            return new CancelReservationCommand(ReservationId: Guid.NewGuid());
        }

        private Reservation CreateDefaultReservation(ReservationStatus status = ReservationStatus.Confirmed)
        {
            var reservation = new Reservation(
                id: Guid.NewGuid(),
                eventId: Guid.NewGuid(),
                quantity: 3,
                buyerName: "Amy Vanessa",
                buyerEmail: "amy.v@mail.com"
            );

            // Forzamos el Status mediante Reflexión
            typeof(Reservation).GetProperty(nameof(Reservation.Status))?.SetValue(reservation, status);

            return reservation;
        }

        private Event CreateDefaultEvent(DateTime startDate)
        {
            return new Event(
                id: Guid.NewGuid(),
                title: "Vallenato VIP",
                description: "Gran noche de acordeones",
                venueId: Guid.NewGuid(),
                maxCapacity: 100,
                startDate: startDate,
                endDate: startDate.AddHours(4),
                ticketPrice: 60.00m,
                eventType: EventType.Conference
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenReservationDoesNotExist()
        {
            // Arrange
            var command = CreateDefaultCommand();
            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns((Reservation?)null);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.NotFound(command.ReservationId));
            await _unitOfWork.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
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
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.AlreadyCancelled);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenReservationIsPendingPayment()
        {
            // Arrange
            var command = CreateDefaultCommand();
            var pendingReservation = CreateDefaultReservation(status: ReservationStatus.Pending);

            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns(pendingReservation);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.PendingPayment);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenEventDoesNotExist()
        {
            // Arrange
            var command = CreateDefaultCommand();
            var reservation = CreateDefaultReservation(status: ReservationStatus.Confirmed);

            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns(reservation);

            _eventRepository.GetByIdAsync(reservation.EventId, Arg.Any<CancellationToken>())
                .Returns((Event?)null);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(EventErrors.NotFound(reservation.EventId));
        }

        [Fact]
        public async Task Handle_Should_CancelWithLostTicketsTrue_WhenEventStartsInLessThan48Hours()
        {
            // Arrange
            var command = CreateDefaultCommand();
            var reservation = CreateDefaultReservation(status: ReservationStatus.Confirmed);

            // El evento empieza en 24 horas (Menor a las 48h de penalización)
            var closeEventDate = DateTime.UtcNow.AddHours(24);
            var @event = CreateDefaultEvent(closeEventDate);

            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns(reservation);

            _eventRepository.GetByIdAsync(reservation.EventId, Arg.Any<CancellationToken>())
                .Returns(@event);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            reservation.Status.Should().Be(ReservationStatus.Cancelled);

            await _unitOfWork.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_CancelWithLostTicketsFalse_WhenEventStartsInMoreThan48Hours()
        {
            // Arrange
            var command = CreateDefaultCommand();
            var reservation = CreateDefaultReservation(status: ReservationStatus.Confirmed);

            // El evento es en 5 días (Más de 48h, cancelación libre de penalidad)
            var farEventDate = DateTime.UtcNow.AddDays(5);
            var @event = CreateDefaultEvent(farEventDate);

            _reservationRepository.GetByIdAsync(command.ReservationId, Arg.Any<CancellationToken>())
                .Returns(reservation);

            _eventRepository.GetByIdAsync(reservation.EventId, Arg.Any<CancellationToken>())
                .Returns(@event);

            // Act
            Result result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            reservation.Status.Should().Be(ReservationStatus.Cancelled);

            await _unitOfWork.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }
    }
}