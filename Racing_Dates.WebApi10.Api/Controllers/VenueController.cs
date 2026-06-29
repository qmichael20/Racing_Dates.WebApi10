using Application.Abstractions.Messaging;
using Application.Venues.Get;
using Domain.Venues;
using Microsoft.AspNetCore.Mvc;
using Racing_Dates.WebApi10.Infrasctruture;
using Web.Api.Extensions;

namespace Racing_Dates.WebApi10.Api.Controllers
{
    [Route("api/venues")]
    [ApiController]
    public class VenueController : ControllerBase
    {
        [HttpGet] 
        public async Task<IActionResult> GetVenuesAsync(
            IQueryHandler<GetVenueCommand,List<Venue>> handler,
            CancellationToken cancellationToken)
        {

            var query = new GetVenueCommand();

            var result =await handler.Handle(query, cancellationToken);

            return result.Match(
                  onSuccess: value => Ok(value),
                  onFailure: error => CustomActionResults.Problem(error));
        }
    }
}
