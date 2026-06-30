using Application.Events.Report;
using Application.Ports;
using Domain.Enums;
using Domain.Events;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using Xunit;

namespace Application.UnitTests.Events.Report
{
    public class ReportEventCommandHandlerTests
    {
        private readonly IEventRepository _eventRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly ReportEventCommandHandler _handler;

        public ReportEventCommandHandlerTests()
        {
            _eventRepository = Substitute.For<IEventRepository>();
            _reservationRepository = Substitute.For<IReservationRepository>();

            _handler = new ReportEventCommandHandler(_eventRepository, _reservationRepository);
        }

        private ReportEventCommand CreateDefaultQuery()
        {
            return new ReportEventCommand(EventId: Guid.NewGuid());
        }

        private Event CreateDefaultEvent(int maxCapacity = 100, decimal ticketPrice = 50.00m)
        {
            return new Event(
                id: Guid.NewGuid(),
                title: "Concierto Champeta",
                description: "Gran concierto bailable",
                venueId: Guid.NewGuid(),
                maxCapacity: maxCapacity,
                startDate: DateTime.UtcNow.AddDays(10),
                endDate: DateTime.UtcNow.AddDays(10).AddHours(4),
                ticketPrice: ticketPrice,
                eventType: EventType.Conference
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnFailure_WhenEventDoesNotExist()
        {
            // Arrange
            var query = CreateDefaultQuery();
            _eventRepository.GetByIdAsync(query.EventId, Arg.Any<CancellationToken>())
                .Returns((Event?)null);

            // Act
            Result<OccupancyReportResponseDto> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeEquivalentTo(EventErrors.NotFound(query.EventId));
        }

        [Fact]
        public async Task Handle_Should_ReturnCalculatedReport_WhenEventExists()
        {
            // Arrange
            var query = CreateDefaultQuery();
            var @event = CreateDefaultEvent(maxCapacity: 200, ticketPrice: 50.00m); // Capacidad: 200, Precio: 50

            _eventRepository.GetByIdAsync(query.EventId, Arg.Any<CancellationToken>())
                .Returns(@event);

            _reservationRepository.GetConfirmedTickets(query.EventId, Arg.Any<CancellationToken>())
                .Returns(100); // 100 vendidos

            _reservationRepository.GetLostTickets(query.EventId, Arg.Any<CancellationToken>())
                .Returns(20);  // 20 perdidos

            // Act
            Result<OccupancyReportResponseDto> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            // Verificaciones matemáticas del Reporte:
            // 1. Entradas disponibles: 200 - 100 - 20 = 80
            result.Value.AvailableTickets.Should().Be(80);

            // 2. Porcentaje de ocupación: (100 * 100) / 200 = 50%
            result.Value.OccupancyPercentage.Should().Be(50.00m);

            // 3. Ganancias totales: 100 vendidos * 50.00m = 5000.00m
            result.Value.TotalRevenue.Should().Be(5000.00m);

            result.Value.EventId.Should().Be(@event.Id);
            result.Value.EventName.Should().Be(@event.Title);
        }

        [Fact]
        public async Task Handle_Should_ReturnZeroOccupancy_WhenMaxCapacityIsZero()
        {
            // Arrange
            var query = CreateDefaultQuery();
            var @event = CreateDefaultEvent(maxCapacity: 0, ticketPrice: 10.00m); // Evento sin capacidad máxima configurada

            _eventRepository.GetByIdAsync(query.EventId, Arg.Any<CancellationToken>())
                .Returns(@event);

            _reservationRepository.GetConfirmedTickets(query.EventId, Arg.Any<CancellationToken>())
                .Returns(0);

            _reservationRepository.GetLostTickets(query.EventId, Arg.Any<CancellationToken>())
                .Returns(0);

            // Act
            Result<OccupancyReportResponseDto> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            // Valida que el operador ternario controló la división por cero de manera segura
            result.Value.OccupancyPercentage.Should().Be(0);
            result.Value.TotalRevenue.Should().Be(0);
        }
    }
}