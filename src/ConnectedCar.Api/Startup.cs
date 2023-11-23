using ConnectedCar.Core.Shared.Services;
using ConnectedCar.Core.Shared.Orchestrators;
using ConnectedCar.Core.Services.Context;
using ConnectedCar.Core.Services.Translator;
using ConnectedCar.Core.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConnectedCar.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceContext, CloudServiceContext>();
            services.AddSingleton<ITranslator, Translator>();
            services.AddSingleton<IDealerService, DealerService>();
            services.AddSingleton<ITimeslotService, TimeslotService>();
            services.AddSingleton<IAppointmentService, AppointmentService>();
            services.AddSingleton<ICustomerService, CustomerService>();
            services.AddSingleton<IRegistrationService, RegistrationService>();
            services.AddSingleton<IVehicleService, VehicleService>();
            services.AddSingleton<IEventService, EventService>();
            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IAdminOrchestrator, AdminOrchestrator>();
            services.AddSingleton<ICustomerOrchestrator, CustomerOrchestrator>();
		
            services.AddControllers().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
