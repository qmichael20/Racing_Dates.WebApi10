using Application.Abstractions.Messaging;
using Application.Ports;
using Domain.Enums;
using Domain.Events;
using Domain.Reservations;
using SharedKernel;

namespace Application.Reservations.Cancel
{
    internal sealed class CancekReservationCommandHandler(
        IReservationRepository reservationRepository,
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork
    ) : ICommandHandler<CancelReservationCommand>
    {
        public async Task<Result> Handle(CancelReservationCommand command, CancellationToken cancellationToken)
        {
            Reservation? reservation = await reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);

            if (reservation is null)
            {
                return Result.Failure(ReservationErrors.NotFound(command.ReservationId));
            }

            if (reservation.Status == ReservationStatus.Cancelled)
            {
                return Result.Failure(ReservationErrors.AlreadyCancelled);
            }

            if (reservation.Status == ReservationStatus.Pending)
            {
                return Result.Failure(ReservationErrors.PendingPayment);
            }

            Event? @event = await eventRepository.GetByIdAsync(reservation.EventId, cancellationToken);

            if (@event is null)
            {
                return Result.Failure(EventErrors.NotFound(reservation.EventId));
            }

            bool lostTickets =(@event.StartDate - DateTime.UtcNow).TotalHours < 48;

            reservation.Cancel(lostTickets);

            await unitOfWork.SaveAsync(cancellationToken);

            return Result.Success();
        }
    }
}
