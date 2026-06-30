using Application.Ports;
using Application.Reservations.Create;
using Domain.Enums;
using Domain.Events;
using Domain.Reservations;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using System.Reflection;
using Xunit;

namespace Application.UnitTests.Reservations.Create
{
    public class CreateReservationCommandHandlerTests
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CreateReservationCommandHandler _handler;

        public CreateReservationCommandHandlerTests()
        {
            _reservationRepository = Substitute.For<IReservationRepository>();
            _eventRepository = Substitute.For<IEventRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _handler = new CreateReservationCommandHandler(
                _reservationRepository,
                _eventRepository,
                _unitOfWork);
        }

        private CreateReservationCommand CreateDefaultCommand(int quantity = 2)
        {
            return new CreateReservationCommand(
                EventId: Guid.NewGuid(),
                Quantity: quantity,
                BuyerName: "Juan Pérez",
                BuyerEmail: "juan.perez@mail.com"
            );
        }

        private Event CreateDefaultEvent(
            int maxCapacity = 100,
            decimal ticketPrice = 50.00m,
            DateTime? startDate = null,
            EventState status = EventState.Active)
        {
            var @event = new Event(
                id: Guid.NewGuid(),
                title: "Gran Liga de la Champeta",
                description: "Evento bailable con el mejor ambiente",
                venueId: Guid.NewGuid(),
                maxCapacity: maxCapacity,
                startDate: startDate ?? DateTime.UtcNow.AddDays(2), // Por defecto en 48 horas (Seguro)
                endDate: startDate?.AddHours(4) ?? DateTime.UtcNow.AddDays(2).AddHours(4),
                ticketPrice: ticketPrice,
                eventType: EventType.Conference
            );

            // Forzamos el Status mediante reflexión por si el setter es privado/init
            typeof(Event).GetProperty(nameof(Event.Status))?.SetValue(@event, status);

            return @event;
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenEventDoesNotExist()
        {
            // Arrange
            var command = CreateDefaultCommand();
            _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>())
                .Returns((Event?)null);

            // Act
            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(EventErrors.NotFound(command.EventId));
            await _unitOfWork.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenEventIsAlreadyCompleted()
        {
            // Arrange
            var command = CreateDefaultCommand();
            var completedEvent = CreateDefaultEvent(status: EventState.Completed);

            _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>())
                .Returns(completedEvent);

            // Act
            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(EventErrors.EventAlreadyCompleted(command.EventId));
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenNotEnoughTicketsAvailable()
        {
            // Arrange
            var command = CreateDefaultCommand(quantity: 10); // Intenta comprar 10
            var @event = CreateDefaultEvent(maxCapacity: 100);

            _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>())
                .Returns(@event);

            _reservationRepository.GetReservedTickets(command.EventId, Arg.Any<CancellationToken>())
                .Returns(95); // Ya hay 95 reservados. Solo quedan 5 disponibles.

            // Act
            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.NotEnoughTickets);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenLessWith24HoursAndQuantityExceedsFive()
        {
            // Arrange
            // El evento empieza en 12 horas (Menor a 24h restricción)
            var closeStartDate = DateTime.UtcNow.AddHours(12);
            var command = CreateDefaultCommand(quantity: 6); // Solicita más de 5
            var @event = CreateDefaultEvent(maxCapacity: 100, startDate: closeStartDate);

            _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>())
                .Returns(@event);

            _reservationRepository.GetReservedTickets(command.EventId, Arg.Any<CancellationToken>())
                .Returns(0);

            // Act
            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.MaxFiveTickets);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenReservationIsLate_LessThanAnHour()
        {
            // Arrange
            // El evento empieza en 30 minutos (Falta menos de 1 hora)
            var lateDate = DateTime.UtcNow.AddMinutes(30);
            var command = CreateDefaultCommand(quantity: 2);
            var @event = CreateDefaultEvent(maxCapacity: 100, startDate: lateDate);

            _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>())
                .Returns(@event);

            _reservationRepository.GetReservedTickets(command.EventId, Arg.Any<CancellationToken>())
                .Returns(0);

            // Act
            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.LateReservation);
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenTicketPriceIsExpensiveAndQuantityExceedsTen()
        {
            // Arrange
            var command = CreateDefaultCommand(quantity: 12); // Pide más de 10
            var expensiveEvent = CreateDefaultEvent(maxCapacity: 100, ticketPrice: 150.00m); // Cuesta > 100

            _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>())
                .Returns(expensiveEvent);

            _reservationRepository.GetReservedTickets(command.EventId, Arg.Any<CancellationToken>())
                .Returns(0);

            // Act
            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(ReservationErrors.MaxTenTickets);
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccessGuid_WhenCommandIsValid()
        {
            // Arrange
            var command = CreateDefaultCommand(quantity: 3);
            var @event = CreateDefaultEvent(maxCapacity: 50, ticketPrice: 40.00m); 
            _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>())
                .Returns(@event);

            _reservationRepository.GetReservedTickets(command.EventId, Arg.Any<CancellationToken>())
                .Returns(10); 

            // Act
            Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();

            // Verificar inserciones y persistencia en DB
            await _reservationRepository.Received(1).AddAsync(
                Arg.Is<Reservation>(r => r.EventId == command.EventId && r.Quantity == command.Quantity),
                Arg.Any<CancellationToken>());

            await _unitOfWork.Received(1).SaveAsync(Arg.Any<CancellationToken>());
        }
    }
}