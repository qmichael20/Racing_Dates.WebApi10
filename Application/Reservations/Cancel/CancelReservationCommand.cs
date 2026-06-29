using Application.Abstractions.Messaging;

namespace Application.Reservations.Cancel
{
    public sealed record CancelReservationCommand(
        Guid ReservationId
    ) : ICommand;
}