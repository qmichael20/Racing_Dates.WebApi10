using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Reservations.Get
{
    public sealed record GetReservationCommand(
        string? BuyerName,
        string? BuyerEmail
    ) : IQuery<List<ReservationResponse>>;

    public sealed record ReservationResponse(
        Guid Id,
        string? ReservationCode,
        Guid EventId,
        string EventTitle,
        string BuyerName,
        string BuyerEmail,
        int Quantity,
        ReservationStatus Status,
        DateTime? CancelledAt,
        bool LostTickets
    );
}
