using Application.Abstractions.Data;
using Application.Ports;
using Application.Reservations.Get;
using Domain.Enums;
using Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Adapters
{
    [Repository]
    public class ReservationRepository(
        IRepository<Reservation> repository,
        IApplicationDbContext context
    ) : IReservationRepository
    {
        public async Task<Guid> AddAsync(Reservation reservation, CancellationToken cancellationToken)
        {
            Reservation added = await repository.AddAsync(reservation, cancellationToken);
            return added.Id;
        }

        public async Task<int> GetReservedTickets(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            return await context.Reservations
                .Where(x =>
                    x.EventId == eventId &&
                    (
                        x.Status == ReservationStatus.Pending ||
                        x.Status == ReservationStatus.Confirmed ||
                        x.LostTickets
                    ))
                .SumAsync(x => x.Quantity, cancellationToken);
        }

        public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await context.Reservations
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken)
        {
            return await context.Reservations
                .AnyAsync(x => x.ReservationCode == code, cancellationToken);
        }

        public async Task<int> GetConfirmedTickets(
           Guid eventId,
           CancellationToken cancellationToken)
        {
            return await context.Reservations
                .Where(x =>
                    x.EventId == eventId &&
                    x.Status == ReservationStatus.Confirmed)
                .SumAsync(x => x.Quantity, cancellationToken);
        }

        public async Task<int> GetLostTickets(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            return await context.Reservations
                .Where(x =>
                    x.EventId == eventId &&
                    x.LostTickets)
                .SumAsync(x => x.Quantity, cancellationToken);
        }

        public async Task<List<Reservation>> GetAsync(
            string? buyerName,
            string? buyerEmail,
            CancellationToken cancellationToken)
        {
            IQueryable<Reservation> query =
                context.Reservations
                    .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buyerName))
            {
                query = query.Where(x =>
                    x.BuyerName.Contains(buyerName));
            }

            if (!string.IsNullOrWhiteSpace(buyerEmail))
            {
                query = query.Where(x =>
                    x.BuyerEmail.Contains(buyerEmail));
            }

            return await query.ToListAsync(
                cancellationToken);
        }
    }
}