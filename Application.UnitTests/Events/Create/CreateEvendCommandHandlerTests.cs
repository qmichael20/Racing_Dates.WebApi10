using Application.Events.Create;
using Application.Ports;
using Domain.Enums;
using Domain.Events;
using Domain.Venues;
using FluentAssertions;
using NSubstitute;
using Xunit;
using SharedKernel;

namespace Application.UnitTests.Events.Create;

public class CreateEventCommandHandlerTests
{
    private readonly IEventRepository _eventRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _eventRepository = Substitute.For<IEventRepository>();
        _venueRepository = Substitute.For<IVenueRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CreateEventCommandHandler(
            _eventRepository,
            _venueRepository,
            _unitOfWork);
    }

    private CreateEventCommand CreateDefaultCommand(
        Guid? venueId = null,
        int maxCapacity = 100,
        DateTime? startDate = null)
    {
        return new CreateEventCommand(
            Title: "Concierto de Rock",
            Description: "Un evento increíble",
            VenueId: venueId ?? Guid.NewGuid(),
            MaxCapacity: maxCapacity,
            StartDate: startDate ?? new DateTime(2026, 7, 15, 18, 0, 0), 
            EndDate: startDate?.AddHours(2) ?? new DateTime(2026, 7, 15, 20, 0, 0),
            TicketPrice: 45.50m,
            EventType: EventType.Conference
        );
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenVenueDoesNotExist()
    {
        // Arrange
        var command = CreateDefaultCommand();
        _venueRepository.GetByIdAsync(command.VenueId, Arg.Any<CancellationToken>())
            .Returns((Venue?)null);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(VenueErrors.NotFound(command.VenueId));
        await _unitOfWork.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenMaxCapacityExceedsVenueCapacity()
    {
        // Arrange
        var command = CreateDefaultCommand(maxCapacity: 500);
        var venue = new Venue(command.VenueId, "Estadio", capacity: 200); // Asumiendo este constructor en tu Venue

        _venueRepository.GetByIdAsync(command.VenueId, Arg.Any<CancellationToken>())
            .Returns(venue);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(EventErrors.InvalidCapacity);
        await _unitOfWork.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenEventsOverlap()
    {
        // Arrange
        var command = CreateDefaultCommand();
        var venue = new Venue(command.VenueId, "Teatro", capacity: 150);

        _venueRepository.GetByIdAsync(command.VenueId, Arg.Any<CancellationToken>())
            .Returns(venue);

        _eventRepository.HasOverlappingEvents(
                command.VenueId, command.StartDate, command.EndDate, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(EventErrors.OverlappingVenue);
        await _unitOfWork.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_WhenWeekendNightRestrictionIsViolated()
    {
        // Arrange
        // Sábado 18 de Julio de 2026 a las 22:30 (Viola la restricción de > 22:00)
        var saturdayNight = new DateTime(2026, 7, 18, 22, 30, 0);
        var command = CreateDefaultCommand(startDate: saturdayNight);
        var venue = new Venue(command.VenueId, "Club", capacity: 100);

        _venueRepository.GetByIdAsync(command.VenueId, Arg.Any<CancellationToken>())
            .Returns(venue);

        _eventRepository.HasOverlappingEvents(
                command.VenueId, command.StartDate, command.EndDate, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(EventErrors.WeekendNightRestriction);
        await _unitOfWork.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_WhenCommandIsValid()
    {
        // Arrange
        // Aseguramos un Martes a las 10:00 AM del año 2026 (Evita 100% la regla del fin de semana)
        var safeDate = new DateTime(2026, 7, 14, 10, 0, 0);
        var command = CreateDefaultCommand(startDate: safeDate, maxCapacity: 100);

        // El Venue DEBE tener la misma o más capacidad que el comando (100)
        var venue = new Venue(command.VenueId, "Auditorio", capacity: 150);

        _venueRepository.GetByIdAsync(command.VenueId, Arg.Any<CancellationToken>())
            .Returns(venue);

        // Aseguramos que NO haya solapamiento
        _eventRepository.HasOverlappingEvents(
                command.VenueId, command.StartDate, command.EndDate, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        Result<Guid> result = await _handler.Handle(command, CancellationToken.None);

        // Assert --- 
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        // Verificar persistencia
        await _eventRepository.Received(1).AddAsync(
            Arg.Is<Event>(e => e.Title == command.Title && e.VenueId == command.VenueId),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }
}