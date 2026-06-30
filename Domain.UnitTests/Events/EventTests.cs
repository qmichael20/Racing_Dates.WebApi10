using Domain.Enums;
using Domain.Events;
using FluentAssertions;
using Xunit;

namespace Domain.UnitTests.Events
{
    public class EventTests
    {
        [Fact]
        public void Constructor_Should_InitializeEventWithActiveStatus_WhenDataIsValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var title = "Festival de Orquestas";
            var description = "Gran evento de música caribeña";
            var venueId = Guid.NewGuid();
            var maxCapacity = 500;
            var startDate = DateTime.UtcNow.AddDays(5);
            var endDate = startDate.AddHours(4);
            var ticketPrice = 45000m;
            var eventType = EventType.Concert;

            // Act
            var @event = new Event(id, title, description, venueId, maxCapacity, startDate, endDate, ticketPrice, eventType);

            // Assert
            @event.Id.Should().Be(id);
            @event.Status.Should().Be(EventState.Active); // Verifica el estado inicial por defecto
        }

        [Fact]
        public void MarkAsCompleted_Should_ChangeStatusToCompleted()
        {
            // Arrange
            var @event = new Event(
                Guid.NewGuid(),
                "Gran Concierto",
                "Descripción válida del evento",
                Guid.NewGuid(),
                100,
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(2).AddHours(2),
                20.00m,
                EventType.Conference
            );

            // Act
            @event.MarkAsCompleted();

            // Assert
            @event.Status.Should().Be(EventState.Completed);
        }
    }
}