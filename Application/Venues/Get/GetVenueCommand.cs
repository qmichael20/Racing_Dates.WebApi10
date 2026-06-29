using Application.Abstractions.Messaging;
using Domain.Venues;

namespace Application.Venues.Get
{
    public sealed record GetVenueCommand() : IQuery<List<Venue>>;
}
