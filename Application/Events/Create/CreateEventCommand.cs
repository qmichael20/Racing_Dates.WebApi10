using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Events.Create
{
    public sealed record CreateEventCommand(
        string Title,
        string Description,
        Guid VenueId,
        int MaxCapacity,
        DateTime StartDate,
        DateTime EndDate,
        decimal TicketPrice,
        EventType EventType
    ) : ICommand<Guid>;
}