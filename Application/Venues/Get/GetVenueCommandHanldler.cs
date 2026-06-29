using Application.Abstractions.Messaging;
using Application.Ports;
using Domain.Venues;
using SharedKernel;

namespace Application.Venues.Get
{
    internal sealed class GetVenueCommandHanldler(
        IVenueRepository venueRepository)
        : IQueryHandler<GetVenueCommand, List<Venue>>
    {
        public async Task<Result<List<Venue>>> Handle(
            GetVenueCommand command,
            CancellationToken cancellationToken)
        {
            List<Venue> venues = await venueRepository.GetAsync(cancellationToken);
            return venues;
        }
    }
}
