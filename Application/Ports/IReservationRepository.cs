using Application.Reservations.Get;
using Domain.Reservations;

namespace Application.Ports
{
    public interface IReservationRepository
    {
        Task<Guid> AddAsync(Reservation reservation, CancellationToken cancellationToken);

        Task<int> GetReservedTickets(Guid eventId, CancellationToken cancellationToken);

        Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);

        Task<int> GetConfirmedTickets(Guid eventId, CancellationToken cancellationToken);

        Task<int> GetLostTickets(Guid eventId, CancellationToken cancellationToken);

        Task<List<Reservation>> GetAsync(string BuyerName, string? BuyerEmail, CancellationToken cancellationToken);
    }
}
