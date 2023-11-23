using System.Net.Mime;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ConnectedCar.Core.Shared.Data.Entities;
using ConnectedCar.Core.Shared.Services;

namespace ConnectedCar.Api.Controllers
{
    [ApiController]
    [Route("vehicle")]
    public class VehicleController : BaseController
    {
        private readonly IEventService eventService;

        public VehicleController(
            ILoggerFactory logger, 
            IEventService eventService) : base(logger)
        {
            this.eventService = eventService;
        }

        [HttpPost("events")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> CreateEvent([FromHeader(Name = "X-Vin")] string vin, [FromBody] Event evnt)
        {
            return await Process(async () =>
            {
                evnt.Vin = vin;
                await eventService.CreateEvent(evnt);

                return Created($"/vehicle/events/{evnt.Timestamp}", null);
            });
        }

        [HttpGet("events")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetEvents([FromHeader(Name = "X-Vin")] string vin)
        {
            return await Process(async () =>
            {
                List<Event> events = await eventService.GetEvents(vin);

                return Ok(events);
            });
        }   

        [HttpGet("events/{timestamp}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetEvent([FromHeader(Name = "X-Vin")] string vin, string timestamp)
        {
            return await Process(async () =>
            {
                Event evnt = await eventService.GetEvent(vin, long.Parse(timestamp));

                if (evnt != null)
                    return Ok(evnt);
                else
                    return NotFound();
            });
        }   
    }
}