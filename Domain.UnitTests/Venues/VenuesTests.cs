using Domain.Venues;
using FluentAssertions;
using Xunit;

namespace Domain.UnitTests.Venues
{
    public class VenueTests
    {
        [Fact]
        public void Constructor_Should_InitializePropertiesCorrectly_WhenAllParametersAreProvided()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            string name = "Teatro Amira de la Rosa";
            int capacity = 500;
            string ciudad = "Barranquilla";

            // Act
            var venue = new Venue(id, name, capacity, ciudad);

            // Assert
            venue.Id.Should().Be(id);
            venue.Name.Should().Be(name);
            venue.Capacity.Should().Be(capacity);
            venue.Ciudad.Should().Be(ciudad);
        }

        [Fact]
        public void Constructor_Should_AssignDefaultCity_WhenCiudadParameterIsOmitted()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            string name = "Plaza de la Paz";
            int capacity = 2000;

            // Act - Omitimos el parámetro 'ciudad' para disparar el valor por defecto
            var venue = new Venue(id, name, capacity);

            // Assert
            venue.Id.Should().Be(id);
            venue.Name.Should().Be(name);
            venue.Capacity.Should().Be(capacity);

            // Verifica que tu regla de parámetro opcional (ciudad = "Default") funcione
            venue.Ciudad.Should().Be("Default");
        }
    }
}