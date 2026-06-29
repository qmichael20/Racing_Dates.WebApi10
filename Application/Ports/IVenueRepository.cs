using Domain.Venues;

namespace Application.Ports
{
    public interface IVenueRepository
    {
        Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<List<Venue>> GetAsync(CancellationToken cancellationToken);
    }
}
