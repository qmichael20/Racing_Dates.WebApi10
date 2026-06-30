using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Ports;
using Domain.Events;
using Domain.Venues;
using SharedKernel;

namespace Application.Events.Create
{
    internal sealed class CreateEventCommandHandler(
        IEventRepository eventRepository,
        IVenueRepository venueRepository,
        IUnitOfWork unitOfWork
    ) : ICommandHandler<CreateEventCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(
            CreateEventCommand command,
            CancellationToken cancellationToken)
        {
            Venue? venue = await venueRepository.GetByIdAsync(command.VenueId, cancellationToken);

            if (venue is null)
            {
                return Result.Failure<Guid>(
                    VenueErrors.NotFound(command.VenueId));
            }

            if (command.MaxCapacity > venue.Capacity)
            {
                return Result.Failure<Guid>(
                    EventErrors.InvalidCapacity);
            }

            bool overlap =
                await eventRepository.HasOverlappingEvents(
                    command.VenueId,
                    command.StartDate,
                    command.EndDate,
                    cancellationToken);

            if (overlap)
            {
                return Result.Failure<Guid>(
                    EventErrors.OverlappingVenue);
            }

            bool weekend =
                command.StartDate.DayOfWeek == DayOfWeek.Saturday ||
                command.StartDate.DayOfWeek == DayOfWeek.Sunday;

            if (weekend && command.StartDate.TimeOfDay > TimeSpan.FromHours(22))
            {
                return Result.Failure<Guid>(
                    EventErrors.WeekendNightRestriction);
            }

            Event newEvent = new(
                id: Guid.NewGuid(),
                title: command.Title,
                description: command.Description,
                venueId: command.VenueId,
                maxCapacity: command.MaxCapacity,
                startDate: command.StartDate,
                endDate: command.EndDate,
                ticketPrice: command.TicketPrice,
                eventType: command.EventType
            );

            await eventRepository.AddAsync(newEvent, cancellationToken);

            await unitOfWork.SaveAsync(cancellationToken);

            return newEvent.Id;
        }
    }
}