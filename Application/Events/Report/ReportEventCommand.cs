using Application.Abstractions.Messaging;
using Domain.Enums;

namespace Application.Events.Report
{
    public sealed record ReportEventCommand(
        Guid EventId
    ) : IQuery<OccupancyReportResponseDto>;

    public sealed record OccupancyReportResponseDto(
    Guid EventId,
    string EventName,
    int SoldTickets,
    int AvailableTickets,
    decimal OccupancyPercentage,
    decimal TotalRevenue,
    EventState Status
);
}
