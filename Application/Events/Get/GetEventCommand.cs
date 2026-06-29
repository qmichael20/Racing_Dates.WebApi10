using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Events.Get
{
    public sealed record GetEventCommand(
        EventType? EventType,
        DateTime? StartDateFrom,
        DateTime? StartDateTo,
        Guid? VenueId,
        EventState? Status,
        string? Title
    ) : IQuery<List<EventResponse>>;

    public sealed record EventResponse(
          Guid Id,
          string Title,
          string Description,
          Guid VenueId,
          string VenueName,
          int MaxCapacity,
          DateTime StartDate,
          DateTime EndDate,
          decimal TicketPrice,
          EventType EventType,
          EventState Status
      );
}