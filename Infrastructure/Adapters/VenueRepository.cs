using Application.Abstractions.Data;
using Application.Ports;
using Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters
{
    [Repository]
    public class VenueRepository(IRepository<Venue> repository, IApplicationDbContext context) : IVenueRepository
    {
        public async Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await context.Venues
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<Venue>> GetAsync(CancellationToken cancellationToken)
        {
            IQueryable<Venue> query =
                context.Venues
                    .AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }
    }
}
