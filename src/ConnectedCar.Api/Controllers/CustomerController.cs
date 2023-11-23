using System.Linq;
using System.Net.Mime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ConnectedCar.Core.Shared.Data.Enums;
using ConnectedCar.Core.Shared.Data.Entities;
using ConnectedCar.Core.Shared.Data.Updates;
using ConnectedCar.Core.Shared.Services;
using ConnectedCar.Core.Shared.Orchestrators;

namespace ConnectedCar.Api.Controllers
{
    [ApiController]
    [Route("customer")]
    public class CustomerController : BaseController
    {
        private readonly IDealerService dealerService;
        private readonly ITimeslotService timeslotService;
        private readonly IAppointmentService appointmentService;
        private readonly ICustomerService customerService;
        private readonly IRegistrationService registrationService;
        private readonly IVehicleService vehicleService;
        private readonly IEventService eventService;
        private readonly ICustomerOrchestrator customerOrchestrator;

        public CustomerController(
            ILoggerFactory logger, 
            IDealerService dealerService,
            ITimeslotService timeslotService,
            IAppointmentService appointmentService,
            ICustomerService customerService,
            IRegistrationService registrationService,
            IVehicleService vehicleService,
            IEventService eventService,
            ICustomerOrchestrator customerOrchestrator) : base(logger)
        {
            this.dealerService = dealerService;
            this.timeslotService = timeslotService;
            this.appointmentService = appointmentService;
            this.customerService = customerService;
            this.registrationService = registrationService;
            this.vehicleService = vehicleService;
            this.eventService = eventService;
            this.customerOrchestrator = customerOrchestrator;
        }

        [HttpPatch("profile")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> UpdateCustomer([FromHeader(Name="authorization")] string authorization, [FromBody] CustomerPatch customerPatch)
        {
            return await Process(async () =>
            {
                string username = GetUsername(authorization);
                customerPatch.Username = username;

                await customerService.UpdateCustomer(customerPatch);

                return Ok();
            });
        }

        [HttpGet("profile")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetCustomer([FromHeader(Name="authorization")] string authorization)
        {
            return await Process(async () =>
            {
                string username = GetUsername(authorization);
                Customer customer = await customerService.GetCustomer(username);

                return Ok(customer);
            });
        }

        /********************************************************************************************/

        [HttpPost("appointments")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> CreateAppointment([FromHeader(Name="authorization")] string authorization, [FromBody] Appointment appointment)
        {
            return await Process(async () =>
            {
                string username = GetUsername(authorization);

                string appointmentId = await customerOrchestrator.CreateAppointment(username, appointment);

                if (appointmentId != null)
                {
                    return Created($"/customer/appointments/{appointmentId}", null);
                }
                
                return BadRequest();
            });
        }

        [HttpDelete("appointments/{appointmentId}")]
        public async Task<ActionResult> DeleteAppointment([FromHeader(Name="authorization")] string authorization, string appointmentId)
        {
            return await Process(async () =>
            {
                string username = GetUsername(authorization);
                Appointment appointment = await appointmentService.GetAppointment(appointmentId);

                if (appointment != null && appointment.RegistrationKey.Username.Equals(username))
                {
                    await appointmentService.DeleteAppointment(appointmentId);
                    return Ok();
                }

                return BadRequest();
            });
        }

        [HttpGet("appointments/{appointmentId}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetAppointment([FromHeader(Name="authorization")] string authorization, string appointmentId)
        {
            return await Process(async () =>
            {
                string username = GetUsername(authorization);
                Appointment appointment = await appointmentService.GetAppointment(appointmentId);

                if (appointment != null && appointment.RegistrationKey.Username.Equals(username))
                {
                    return Ok(appointment);
                }

                return NotFound();
            });
        }

        /********************************************************************************************/

        [HttpGet("registrations")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetRegistrations([FromHeader(Name="authorization")] string authorization)
        {
            return await Process(async () =>
            {
                string username = GetUsername(authorization);
                List<Registration> registrations = await registrationService.GetCustomerRegistrations(username);

                return Ok(registrations);
            });
        }

        [HttpGet("registrations/{vin}/appointments")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetAppointments([FromHeader(Name="authorization")] string authorization, string vin)
        {
            return await Process(async () =>
            {
                string username = GetUsername(authorization);
                List<Appointment> appointments = await appointmentService.GetRegistrationAppointments(username, vin);

                return Ok(appointments);
            });
        }

        /********************************************************************************************/

        [HttpGet("vehicles/{vin}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetVehicle([FromHeader(Name="authorization")] string authorization, string vin)
        {
            return await Process(async () =>
            {
                string username = GetUsername(authorization);

                Vehicle vehicle = await customerOrchestrator.GetVehicle(username, vin);

                if (vehicle != null)
                {
                    return Ok(vehicle);
                }

                return NotFound();
            });
        }

        [HttpGet("vehicles/{vin}/events")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetEvents([FromHeader(Name="authorization")] string authorization, string vin)
        {
            return await Process(async () =>
            {
                string username = GetUsername(authorization);
                
                List<Event> events = await customerOrchestrator.GetEvents(username, vin);

                return Ok(events);
            });
        }

        /********************************************************************************************/

        [HttpGet("dealers")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetDealers([FromQuery(Name="stateCode")] string code)
        {
            return await Process(async () =>
            {
                StateCodeEnum stateCode = (StateCodeEnum)Enum.Parse(typeof(StateCodeEnum), code);
                List<Dealer> dealers = await dealerService.GetDealers(stateCode);

                return Ok(dealers);
            });
        }

        [HttpGet("dealers/{dealerId}/timeslots")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetTimeslots(string dealerId)
        {
            return await Process(async () =>
            {
                const string DateFormat = "yyyy-MM-dd";

                string startDate = DateTime.Now.ToString(DateFormat);
                string endDate = DateTime.Now.AddDays(30).ToString(DateFormat);

                List<Timeslot> timeslots = await timeslotService.GetTimeslots(dealerId, startDate, endDate);

                return Ok(timeslots);
            });
        }

        /********************************************************************************************/

        private string GetUsername(string authorization)
        {
            string jwt = authorization.Substring(7);
            var claims = GetUnvalidatedClaims(jwt);
            var claim = claims.FirstOrDefault(c => c.Type == "username");
            return claim.Value;
        }

        private IEnumerable<Claim> GetUnvalidatedClaims(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            return token.Claims;
        }
    }
}
