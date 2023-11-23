using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ConnectedCar.Api.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : BaseController
    {
        public HealthController(ILoggerFactory logger) : base(logger)
        {
        }

        [HttpGet("check")]
        [Produces(MediaTypeNames.Application.Json)]
        public ActionResult Check()
        {
            return Ok();
        }   
    }
}