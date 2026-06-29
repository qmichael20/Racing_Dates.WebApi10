using Application.Abstractions.Messaging;
using Application.Ports;
using Domain.Enums;
using Domain.Events;
using Domain.Reservations;
using SharedKernel;

namespace Application.Reservations.Create
{
    internal sealed class CreateReservationCommandHandler(
        IReservationRepository reservationRepository,
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork
    ): ICommandHandler<CreateReservationCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(CreateReservationCommand command, CancellationToken cancellationToken)
        {
            Event? @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

            if (@event is null) { 
                return Result.Failure<Guid>(EventErrors.NotFound(command.EventId));
            }

            if(@event.Status == EventState.Completed)
            {
                return Result.Failure<Guid>(EventErrors.EventAlreadyCompleted(command.EventId));
            }

            int reserved = await reservationRepository.GetReservedTickets(command.EventId, cancellationToken);

            int availableTickets = @event.MaxCapacity - reserved;
            
            if (command.Quantity > availableTickets)
            {
                return Result.Failure<Guid>(ReservationErrors.NotEnoughTickets);
            }

            double hours = (@event.StartDate - DateTime.UtcNow).TotalHours;

            if(hours< 24 && command.Quantity > 5)
            {
                return Result.Failure<Guid>(ReservationErrors.MaxFiveTickets);
            }

            var hours2 = (@event.StartDate - DateTime.UtcNow).TotalHours;

            if (hours2 < 1)
            {
                return Result.Failure<Guid>(
                    ReservationErrors.LateReservation);
            }

            if (@event.TicketPrice > 100 && command.Quantity > 10)
            {
                return Result.Failure<Guid>(
                    ReservationErrors.MaxTenTickets);
            }

            Reservation reservation = new Reservation(
                id: Guid.NewGuid(),
                command.EventId,
                command.Quantity,
                command.BuyerName,
                command.BuyerEmail
            );

            await reservationRepository.AddAsync(reservation, cancellationToken);

            await unitOfWork.SaveAsync();

            return reservation.Id;
        }
    }
}
