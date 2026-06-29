using Domain.Events;
using Domain.Reservations;
using Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data
{
    public interface IApplicationDbContext
    {
        DbSet<Event> Events { get; }
        DbSet<Reservation> Reservations { get; }
        DbSet<Venue> Venues { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}