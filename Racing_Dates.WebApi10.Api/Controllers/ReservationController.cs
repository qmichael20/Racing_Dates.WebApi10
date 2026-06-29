using Application.Abstractions.Messaging;
using Application.Reservations.Cancel;
using Application.Reservations.Confirm;
using Application.Reservations.Create;
using Application.Reservations.Get;
using Microsoft.AspNetCore.Mvc;
using Racing_Dates.WebApi10.Infrasctruture;
using Web.Api.Extensions;

namespace Racing_Dates.WebApi10.Api.Controllers
{
    [Route("api/reservations")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreateReservationCommand command,
            ICommandHandler<CreateReservationCommand, Guid> handler,
            CancellationToken cancellationToken)
        {
            var result =
                await handler.Handle(command, cancellationToken);

            return result.Match(
                onSuccess: value => Ok(value),
                onFailure: error => CustomActionResults.Problem(error));
        }

        [HttpPut("{reservationId}/confirm")]
        public async Task<IActionResult> ConfirmAsync(
            Guid reservationId,
            ICommandHandler<ConfirmReservationCommand, string> handler,
            CancellationToken cancellationToken)
        {
            var command =
                new ConfirmReservationCommand(reservationId);

            var result =
                await handler.Handle(command, cancellationToken);

            return result.Match(
                onSuccess: value => Ok(value),
                onFailure: error => CustomActionResults.Problem(error));
        }

        [HttpPut("{reservationId}/cancel")]
        public async Task<IActionResult> CancelAsync(
            Guid reservationId,
            ICommandHandler<CancelReservationCommand> handler,
            CancellationToken cancellationToken)
        {
            CancelReservationCommand command =
                new CancelReservationCommand(reservationId);

            var result =
                await handler.Handle(command, cancellationToken);

            return result.Match(
                onSuccess: Ok,
                onFailure: CustomActionResults.Problem);
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(
            [FromQuery] string? buyerName,
            [FromQuery] string? buyerEmail,
            IQueryHandler<
                GetReservationCommand,
                List<ReservationResponse>> handler,
            CancellationToken cancellationToken)
        {
            var query =
                new GetReservationCommand(
                    buyerName,
                    buyerEmail);

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


