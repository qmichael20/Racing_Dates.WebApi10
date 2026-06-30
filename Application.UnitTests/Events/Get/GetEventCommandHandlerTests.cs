using Application.Events.Get;
using Application.Ports;
using Domain.Enums;
using Domain.Events;
using Domain.Venues;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using System.Reflection;
using Xunit;

namespace Application.UnitTests.Events.Get
{
    public class GetEventCommandHandlerTests
    {
        private readonly IEventRepository _eventRepository;
        private readonly GetEventsCommandHandler _handler;

        public GetEventCommandHandlerTests()
        {
            _eventRepository = Substitute.For<IEventRepository>();
            _handler = new GetEventsCommandHandler(_eventRepository);
        }

        private GetEventCommand CreateDefaultQuery()
        {
            return new GetEventCommand(
                EventType: EventType.Conference,
                StartDateFrom: new DateTime(2026, 7, 1),
                StartDateTo: new DateTime(2026, 7, 31),
                VenueId: Guid.NewGuid(),
                Status: EventState.Completed,
                Title: "Concierto"
            );
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoEventsMatchFilters()
        {
            // Arrange
            var query = CreateDefaultQuery();

            _eventRepository.GetAsync(
                query.EventType,
                query.StartDateFrom,
                query.StartDateTo,
                query.VenueId,
                query.Status,
                query.Title,
                Arg.Any<CancellationToken>())
            .Returns(new List<Event>());

            // Act
            Result<List<EventResponse>> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_ReturnMappedEventResponses_WhenEventsExist()
        {
            // Arrange
            var query = CreateDefaultQuery();
            // Usamos el constructor de Venue que nos mostraste (incluyendo la ciudad por defecto)
            var venue = new Venue(query.VenueId!.Value, "Teatro Amira de la Rosa", capacity: 500, ciudad: "Barranquilla");

            var @event = new Event(
                id: Guid.NewGuid(),
                title: "Festival de Jazz",
                description: "Gran festival musical",
                venueId: venue.Id,
                maxCapacity: 300,
                startDate: DateTime.UtcNow.AddDays(5),
                endDate: DateTime.UtcNow.AddDays(5).AddHours(3),
                ticketPrice: 80.00m,
                eventType: EventType.Conference
            );

            // SOLUClÓN: Asignamos la propiedad de navegación 'Venue' por medio de Reflexión
            typeof(Event)
                .GetProperty(nameof(Event.Venue))?
                .SetValue(@event, venue);

            var eventList = new List<Event> { @event };

            _eventRepository.GetAsync(
                query.EventType,
                query.StartDateFrom,
                query.StartDateTo,
                query.VenueId,
                query.Status,
                query.Title,
                Arg.Any<CancellationToken>())
            .Returns(eventList);

            // Act
            Result<List<EventResponse>> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);

            var actualResponse = result.Value.First();

            actualResponse.Id.Should().Be(@event.Id);
            actualResponse.Title.Should().Be(@event.Title);
            actualResponse.VenueName.Should().Be(venue.Name); // Comprueba que leyó el objeto inyectado
            actualResponse.TicketPrice.Should().Be(@event.TicketPrice);
        }

        [Fact]
        public async Task Handle_Should_ReturnDefaultLocationName_WhenVenueIsNull()
        {
            // Arrange
            var query = CreateDefaultQuery();

            var eventWithoutVenue = new List<Event>
            {
                new Event(
                    id: Guid.NewGuid(),
                    title: "Taller Tech",
                    description: "Charla de desarrollo de software",
                    venueId: Guid.NewGuid(),
                    maxCapacity: 50,
                    startDate: DateTime.UtcNow.AddDays(2),
                    endDate: DateTime.UtcNow.AddDays(2).AddHours(2),
                    ticketPrice: 0.00m,
                    eventType: EventType.Conference
                )
            };

            _eventRepository.GetAsync(
                query.EventType,
                query.StartDateFrom,
                query.StartDateTo,
                query.VenueId,
                query.Status,
                query.Title,
                Arg.Any<CancellationToken>())
            .Returns(eventWithoutVenue);

            // Act
            Result<List<EventResponse>> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var actualResponse = result.Value.First();

            // Verifica que el operador ternario devuelva la cadena por defecto al no haber un Venue
            actualResponse.VenueName.Should().Be("Location to be confirmed");
        }
    }
}