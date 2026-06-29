using Application.Abstractions.Messaging;

namespace Application.Reservations.Create
{
    public sealed record CreateReservationCommand
    (
        Guid EventId,
        int Quantity,
        string BuyerName,
         string BuyerEmail
    ) : ICommand<Guid>;
}
