using Application.Abstractions.Messaging;
using Application.Ports;
using SharedKernel;

namespace Application.Reservations.Get
{
    internal sealed class GetReservationCommandHandler(
        IReservationRepository reservationRepository,
        IEventRepository eventRepository)
        : IQueryHandler<GetReservationCommand, List<ReservationResponse>>
    {
        public async Task<Result<List<ReservationResponse>>> Handle(
            GetReservationCommand command,
            CancellationToken cancellationToken)
        {
            var reservations = await reservationRepository.GetAsync(
                command.BuyerName,
                command.BuyerEmail,
                cancellationToken);

            var events = await eventRepository.GetAsync(
                cancellationToken);

            List<ReservationResponse> response =
                reservations.Select(x =>
                {
                    var @event = events.FirstOrDefault(
                        e => e.Id == x.EventId);

                    return new ReservationResponse(
                        x.Id,
                        x.ReservationCode,
                        x.EventId,
                        @event?.Title ?? string.Empty,
                        x.BuyerName,
                        x.BuyerEmail,
                        x.Quantity,
                        x.Status,
                        x.CancelledAt,
                        x.LostTickets
                    );
                })
                .ToList();

            return response;
        }
    }
}