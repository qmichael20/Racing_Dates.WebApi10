using Application.Abstractions.Messaging;
using Application.Events.Create;
using Application.Events.Get;
using Application.Events.Report;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Racing_Dates.WebApi10.Infrasctruture;
using SharedKernel;
using Web.Api.Extensions;

namespace Racing_Dates.WebApi10.Api.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreateEventCommand command,
            ICommandHandler<CreateEventCommand, Guid> handler,
            CancellationToken cancellationToken)
        {
            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                onSuccess: value => Ok(value),
                onFailure: error => CustomActionResults.Problem(error));
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(
        [FromQuery] EventType? eventType,
        [FromQuery] DateTime? startDateFrom,
        [FromQuery] DateTime? startDateTo,
        [FromQuery] Guid? venueId,
        [FromQuery] EventState? status,
        [FromQuery] string? title,
        IQueryHandler<GetEventCommand, List<EventResponse>> handler,
        CancellationToken cancellationToken)
            {
                var query = new GetEventCommand(
                    eventType,
                    startDateFrom,
                    startDateTo,
                    venueId,
                    status,
                    title);

            Result<List<EventResponse>> result =
               await handler.Handle(query, cancellationToken);

            return result.Match(
                onSuccess: value => Ok(value),
                onFailure: error => CustomActionResults.Problem(error));
        }

        [HttpGet("{eventId}/occupancy-report")]
        public async Task<IActionResult> GetOccupancyReport(
            Guid eventId,
            IQueryHandler<
                ReportEventCommand,
                OccupancyReportResponseDto> handler,
            CancellationToken cancellationToken)
        {
            var query =
                new ReportEventCommand(eventId);

            var result =
                await handler.Handle(
                    query,
                    cancellationToken);

            return result.Match(
                onSuccess: value => Ok(value),
                onFailure: error => CustomActionResults.Problem(error));
        }
    }
}
