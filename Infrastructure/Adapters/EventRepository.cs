using Application.Abstractions.Data;
using Application.Ports;
using Domain.Enums;
using Domain.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters
{
    [Repository]
    public class EventRepository(IRepository<Event> repository, IApplicationDbContext context) : IEventRepository
    {

        public async Task<Guid> AddAsync(Event @event, CancellationToken cancellationToken)
        {
            Event addedEvent = await repository.AddAsync(@event, cancellationToken);
            return @event.Id;
        }

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await context.Events
                .AsNoTracking()
                .Include(x => x.Venue)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<Event>> GetAsync(
                   EventType? eventType,
                   DateTime? startDateFrom,
                   DateTime? startDateTo,
                   Guid? venueId,
                   EventState? status,
                   string? title,
                   CancellationToken cancellationToken)
        {
            IQueryable<Event> query = context.Events
                        .AsNoTracking()
                        .Include(x => x.Venue);

            if (eventType.HasValue)
            {
                query = query.Where(x =>
                    x.EventType == eventType.Value);
            }

            if (startDateFrom.HasValue)
            {
                query = query.Where(x =>
                    x.StartDate >= startDateFrom.Value);
            }

            if (startDateTo.HasValue)
            {
                query = query.Where(x =>
                    x.StartDate <= startDateTo.Value);
            }

            if (venueId.HasValue)
            {
                query = query.Where(x =>
                    x.VenueId == venueId);
            }

            if (status.HasValue)
            {
                query = query.Where(x =>
                    x.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                title = title.ToLower();

                query = query.Where(x =>
                    x.Title.ToLower().Contains(title));
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<List<Event>> GetAsync(
            CancellationToken cancellationToken)
        {
            return await context.Events
                .AsNoTracking()
                .Include(x => x.Venue) 
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasOverlappingEvents(Guid venueId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            return await context.Events.AnyAsync(
                x =>
                    x.VenueId == venueId &&
                    x.Status == EventState.Active &&
                    x.StartDate < endDate &&
                    startDate < x.EndDate,
                cancellationToken);
        }
    }
}