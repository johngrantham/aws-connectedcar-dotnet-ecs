using System.Net.Mime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [Route("admin")]
    public class AdminController : BaseController
    {
        private readonly IDealerService dealerService;
        private readonly ITimeslotService timeslotService;
        private readonly IAppointmentService appointmentService;
        private readonly ICustomerService customerService;
        private readonly IRegistrationService registrationService;
        private readonly IVehicleService vehicleService;
        private readonly IMessageService messageService;
        private readonly IAdminOrchestrator adminOrchestrator;

        public AdminController(
            ILoggerFactory logger, 
            IDealerService dealerService,
            ITimeslotService timeslotService,
            IAppointmentService appointmentService,
            ICustomerService customerService,
            IRegistrationService registrationService,
            IVehicleService vehicleService,
            IMessageService messageService,
            IAdminOrchestrator adminOrchestrator) : base(logger)
        {
            this.dealerService = dealerService;
            this.timeslotService = timeslotService;
            this.appointmentService = appointmentService;
            this.customerService = customerService;
            this.registrationService = registrationService;
            this.vehicleService = vehicleService;
            this.messageService = messageService;
            this.adminOrchestrator = adminOrchestrator;
        }

        [HttpPost("dealers")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> CreateDealer([FromBody] Dealer dealer)
        {
            return await Process(async () =>
            {
                string dealerId = await dealerService.CreateDealer(dealer);

                return Created($"/admin/dealers/{dealerId}", null);
            });
        }

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

        [HttpGet("dealers/{dealerId}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetDealer(string dealerId)
        {
            return await Process(async () =>
            {
                Dealer dealer = await dealerService.GetDealer(dealerId);

                if (dealer != null)
                    return Ok(dealer);
                else
                    return NotFound();
            });
        }

        /********************************************************************************************/

        [HttpPost("dealers/{dealerId}/timeslots")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> CreateTimeslot([FromBody] Timeslot timeslot)
        {
            return await Process(async () =>
            {
                await timeslotService.CreateTimeslot(timeslot);

                return Created($"/admin/dealers/{timeslot.DealerId}/timeslots/{timeslot.ServiceDateHour}", null);
            });
        }

        [HttpGet("dealers/{dealerId}/timeslots")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetTimeslots(string dealerId, [FromQuery(Name="startDate")] string startDate, [FromQuery(Name="endDate")] string endDate)
        {
            return await Process(async () =>
            {
                List<Timeslot> timeslots = await timeslotService.GetTimeslots(dealerId, startDate, endDate);

                return Ok(timeslots);
            });
        }

        [HttpGet("dealers/{dealerId}/timeslots/{serviceDateHour}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetTimeslot(string dealerId, string serviceDateHour)
        {
            return await Process(async () =>
            {
                Timeslot timeslot = await timeslotService.GetTimeslot(dealerId, serviceDateHour);

                if (timeslot != null)
                    return Ok(timeslot);
                else
                    return NotFound();
            });
        }

        /********************************************************************************************/

        [HttpPost("customers")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> CreateCustomer([FromBody] CustomerProvision provision)
        {
            return await Process(async () =>
            {
                await adminOrchestrator.CreateCustomerUsingMessage(provision);

                return Created($"/admin/customers/{provision.Username}", null);
            });
        }

        [HttpGet("customers")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetCustomers([FromQuery(Name="lastname")] string lastname)
        {
            return await Process(async () =>
            {
                List<Customer> customers = await customerService.GetCustomers(lastname);

                return Ok(customers);
            });
        }

        [HttpGet("customers/{username}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetCustomer(string username)
        {
            return await Process(async () =>
            {
                Customer customer = await customerService.GetCustomer(username);

                if (customer != null)
                    return Ok(customer);
                else
                    return NotFound();
            });
        }

        /********************************************************************************************/

        [HttpPost("customers/{username}/registrations")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> CreateRegistration([FromBody] Registration registration)
        {
            return await Process(async () =>
            {
                await registrationService.CreateRegistration(registration);

                return Created($"/admin/customers/{registration.Username}/registrations/{registration.Vin}", null);
            });
        }

        [HttpPatch("customers/{username}/registrations")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> UpdateRegistration([FromHeader(Name="authorization")] string authorization, [FromBody] RegistrationPatch patch, string username)
        {
            return await Process(async () =>
            {
                patch.Username = username;

                await registrationService.UpdateRegistration(patch);

                return Ok();
            });
        }

        [HttpGet("customers/{username}/registrations")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetRegistrations(string username)
        {
            return await Process(async () =>
            {
                List<Registration> registrations = await registrationService.GetCustomerRegistrations(username);

                return Ok(registrations);
            });
        }

        [HttpGet("customers/{username}/registrations/{vin}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetRegistration(string username, string vin)
        {
            return await Process(async () =>
            {
                Registration registration = await registrationService.GetRegistration(username, vin);

                if (registration != null)
                    return Ok(registration);
                else
                    return NotFound();
            });
        }

        /********************************************************************************************/

        [HttpPost("vehicles")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> CreateVehicle([FromBody] Vehicle vehicle)
        {
            return await Process(async () =>
            {
                await vehicleService.CreateVehicle(vehicle);

                return Created($"/admin/vehicles/{vehicle.Vin}", null);
            });
        }

        [HttpGet("vehicles/{vin}")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetVehicle(string vin)
        {
            return await Process(async () =>
            {
                Vehicle vehicle = await vehicleService.GetVehicle(vin);

                if (vehicle != null)
                    return Ok(vehicle);
                else
                    return NotFound();
            });
        }

        [HttpGet("vehicles/{vin}/registrations")]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> GetVehicleRegistrations(string vin)
        {
            return await Process(async () =>
            {
                List<Registration> registrations = await registrationService.GetVehicleRegistrations(vin);

                return Ok(registrations);
            });
        }
    }
}
