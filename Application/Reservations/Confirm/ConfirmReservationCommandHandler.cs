using Application.Abstractions.Messaging;
using Application.Ports;
using Domain.Enums;
using Domain.Reservations;
using SharedKernel;

namespace Application.Reservations.Confirm
{
    internal sealed class ConfirmReservationCommandHandler(
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork
    ): ICommandHandler<ConfirmReservationCommand, string>
    {
        public async Task<Result<string>> Handle(ConfirmReservationCommand command, CancellationToken cancellationToken)
        {
            var reservation = await reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);

            if (reservation is null)
            {
                return Result.Failure<string>(ReservationErrors.NotFound(command.ReservationId));
            }

            if(reservation.Status == ReservationStatus.Confirmed)
            {
                return Result.Failure<string>(ReservationErrors.AlreadyConfirmed);
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return Result.Failure<string>(ReservationErrors.AlreadyCancelled);
            }

            string code;

            do
            {
                code = $"EV-{Random.Shared.Next(0, 1000000):D6}";
            }
            while (await reservationRepository.CodeExistsAsync(code, cancellationToken));

            reservation.Confirm(code);

            await unitOfWork.SaveAsync(cancellationToken);

            return code;
        }
    }
}
