using Domain.Enums;
using Domain.Events;

namespace Application.Ports
{
    public interface IEventRepository
    {
        Task<Guid> AddAsync(Event @event, CancellationToken cancellationToken);

        Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<List<Event>> GetAsync(
           EventType? eventType,
           DateTime? startDateFrom,
           DateTime? startDateTo,
           Guid? venueId,
           EventState? status,
           string? title,
           CancellationToken cancellationToken);

        Task<List<Event>> GetAsync(CancellationToken cancellationToken);

        Task<bool> HasOverlappingEvents(Guid venueId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    }
}