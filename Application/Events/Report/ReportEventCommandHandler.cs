using Application.Abstractions.Messaging;
using Application.Ports;
using Domain.Events;
using SharedKernel;

namespace Application.Events.Report
{
    internal sealed class ReportEventCommandHandler(
    IEventRepository eventRepository,
    IReservationRepository reservationRepository)
    : IQueryHandler<ReportEventCommand, OccupancyReportResponseDto>
    {
        public async Task<Result<OccupancyReportResponseDto>> Handle(
       ReportEventCommand query,
       CancellationToken cancellationToken)
        {
            Event? @event =
                await eventRepository.GetByIdAsync(
                    query.EventId,
                    cancellationToken);

            if (@event is null)
            {
                return Result.Failure<OccupancyReportResponseDto>(
                    EventErrors.NotFound(query.EventId));
            }

            int soldTickets =
                await reservationRepository.GetConfirmedTickets(
                    query.EventId,
                    cancellationToken);

            int lostTickets =
                await reservationRepository.GetLostTickets(
                    query.EventId,
                    cancellationToken);

            int availableTickets = @event.MaxCapacity - soldTickets - lostTickets;

            decimal occupancyPercentage =
                @event.MaxCapacity == 0
                    ? 0
                    : (decimal)soldTickets * 100 /
                      @event.MaxCapacity;

            decimal totalRevenue = soldTickets * @event.TicketPrice;

            OccupancyReportResponseDto response =
                new OccupancyReportResponseDto(
                    @event.Id,
                    @event.Title,
                    soldTickets,
                    availableTickets,
                    occupancyPercentage,
                    totalRevenue,
                    @event.Status);

            return response;
        }
    }
}
