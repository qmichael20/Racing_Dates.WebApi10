using Application.Ports;
using Application.Reservations.Get;
using Domain.Enums;
using Domain.Events;
using Domain.Reservations;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using Xunit;

namespace Application.UnitTests.Reservations.Get
{
    public class GetReservationCommandHandlerTests
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IEventRepository _eventRepository;
        private readonly GetReservationCommandHandler _handler;

        public GetReservationCommandHandlerTests()
        {
            _reservationRepository = Substitute.For<IReservationRepository>();
            _eventRepository = Substitute.For<IEventRepository>();

            _handler = new GetReservationCommandHandler(_reservationRepository, _eventRepository);
        }

        private GetReservationCommand CreateDefaultQuery()
        {
            return new GetReservationCommand(
                BuyerName: "Carlos Alberto",
                BuyerEmail: "carlos@mail.com"
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoReservationsMatchFilters()
        {
            // Arrange
            var query = CreateDefaultQuery();

            _reservationRepository.GetAsync(query.BuyerName, query.BuyerEmail, Arg.Any<CancellationToken>())
                .Returns(new List<Reservation>());

            _eventRepository.GetAsync(Arg.Any<CancellationToken>())
                .Returns(new List<Event>());

            // Act
            Result<List<ReservationResponse>> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_ReturnMappedResponsesWithEventTitle_WhenReservationsAndEventsExist()
        {
            // Arrange
            var query = CreateDefaultQuery();
            var eventId = Guid.NewGuid();

            var reservations = new List<Reservation>
            {
                new Reservation(
                    id: Guid.NewGuid(),
                    eventId: eventId,
                    quantity: 3,
                    buyerName: query.BuyerName,
                    buyerEmail: query.BuyerEmail
                )
            };

            var events = new List<Event>
            {
                new Event(
                    id: eventId,
                    title: "Festival de la Leyenda Vallenata",
                    description: "El festival más esperado del año",
                    venueId: Guid.NewGuid(),
                    maxCapacity: 500,
                    startDate: DateTime.UtcNow.AddDays(15),
                    endDate: DateTime.UtcNow.AddDays(15).AddHours(5),
                    ticketPrice: 120.00m,
                    eventType: EventType.Conference
                )
            };

            _reservationRepository.GetAsync(query.BuyerName, query.BuyerEmail, Arg.Any<CancellationToken>())
                .Returns(reservations);

            _eventRepository.GetAsync(Arg.Any<CancellationToken>())
                .Returns(events);

            // Act
            Result<List<ReservationResponse>> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);

            var actualResponse = result.Value.First();
            var expectedReservation = reservations.First();
            var expectedEvent = events.First();

            actualResponse.Id.Should().Be(expectedReservation.Id);
            actualResponse.EventId.Should().Be(expectedReservation.EventId);
            actualResponse.EventTitle.Should().Be(expectedEvent.Title); // Verifica que cruzó bien el título por EventId
            actualResponse.BuyerName.Should().Be(expectedReservation.BuyerName);
            actualResponse.Quantity.Should().Be(expectedReservation.Quantity);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyEventTitle_WhenEventIdDoesNotMatchAnyEvent()
        {
            // Arrange
            var query = CreateDefaultQuery();

            var reservations = new List<Reservation>
            {
                new Reservation(
                    id: Guid.NewGuid(),
                    eventId: Guid.NewGuid(), // Un Id aleatorio que no estará en la lista de eventos
                    quantity: 1,
                    buyerName: query.BuyerName,
                    buyerEmail: query.BuyerEmail
                )
            };

            // Lista vacía de eventos para forzar el flujo del null-coalescing (@event?.Title ?? string.Empty)
            _eventRepository.GetAsync(Arg.Any<CancellationToken>())
                .Returns(new List<Event>());

            _reservationRepository.GetAsync(query.BuyerName, query.BuyerEmail, Arg.Any<CancellationToken>())
                .Returns(reservations);

            // Act
            Result<List<ReservationResponse>> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);

            var actualResponse = result.Value.First();
            actualResponse.EventTitle.Should().Be(string.Empty); 
        }
    }
}