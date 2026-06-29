using Application.Abstractions.Messaging;

namespace Application.Reservations.Confirm
{
    public sealed record ConfirmReservationCommand(
        Guid ReservationId
    ) : ICommand<string>;
}
