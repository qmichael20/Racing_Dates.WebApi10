using Application.Ports;
using Application.Venues.Get;
using Domain.Venues;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using Xunit;

namespace Application.UnitTests.Venues.Get
{
    public class GetVenueCommandHandlerTests
    {
        private readonly IVenueRepository _venueRepository;
        private readonly GetVenueCommandHanldler _handler;

        public GetVenueCommandHandlerTests()
        {
            _venueRepository = Substitute.For<IVenueRepository>();
            _handler = new GetVenueCommandHanldler(_venueRepository);
        }

        private GetVenueCommand CreateDefaultQuery()
        {
            return new GetVenueCommand();
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoVenuesExist()
        {
            // Arrange
            var query = CreateDefaultQuery();

            _venueRepository.GetAsync(Arg.Any<CancellationToken>())
                .Returns(new List<Venue>());

            // Act
            Result<List<Venue>> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_Should_ReturnVenuesList_WhenVenuesExist()
        {
            // Arrange
            var query = CreateDefaultQuery();

            var venuesList = new List<Venue>
            {
                new Venue(Guid.NewGuid(), "Estadio Metropolitano", capacity: 46000, ciudad: "Barranquilla"),
                new Venue(Guid.NewGuid(), "Movistar Arena", capacity: 14000, ciudad: "Bogotá")
            };

            _venueRepository.GetAsync(Arg.Any<CancellationToken>())
                .Returns(venuesList);

            // Act
            Result<List<Venue>> result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
            result.Value.Should().BeEquivalentTo(venuesList); // Verifica que los objetos devueltos sean idénticos
        }
    }
}