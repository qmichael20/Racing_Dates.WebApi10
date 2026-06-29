using Application.Abstractions.Messaging;
using Application.Ports;
using SharedKernel;

namespace Application.Events.Get
{
    internal sealed class GetEventsCommandHandler(
        IEventRepository repository)
        : IQueryHandler<GetEventCommand, List<EventResponse>>
    {
        public async Task<Result<List<EventResponse>>> Handle(
            GetEventCommand query,
            CancellationToken cancellationToken)
        {
            var events = await repository.GetAsync(
                query.EventType,
                query.StartDateFrom,
                query.StartDateTo,
                query.VenueId,
                query.Status,
                query.Title,
                cancellationToken);

            List<EventResponse> response =
                events.Select(x =>
                    new EventResponse(
                        x.Id,
                        x.Title,
                        x.Description,
                        x.VenueId,
                        x.Venue != null ? x.Venue.Name : "Location to be confirmed",
                        x.MaxCapacity,
                        x.StartDate,
                        x.EndDate,
                        x.TicketPrice,
                        x.EventType,
                        x.Status
                    ))
                .ToList();

            return response;
        }
    }
}