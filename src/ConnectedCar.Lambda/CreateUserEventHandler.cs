using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using ConnectedCar.Core.Shared.Data;
using ConnectedCar.Core.Shared.Services;
using ConnectedCar.Core.Services.Context;
using ConnectedCar.Core.Services.Translator;
using ConnectedCar.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConnectedCar.Lambda
{
    public class CreateUserEventHandler
    {
        private readonly ServiceProvider serviceProvider;

        public CreateUserEventHandler()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IServiceContext, CloudServiceContext>();
            services.AddSingleton<ITranslator, Translator>();
            services.AddSingleton<IUserService, UserService>();

            serviceProvider = services.BuildServiceProvider();
        }

        public async Task HandleEvent(SQSEvent sqsEvent, ILambdaContext context)
        {
            context.Logger.Log("Received " + sqsEvent.Records.Count + " messages");

            try
            {
                foreach (var record in sqsEvent.Records)
                {
                    User user = JsonConvert.DeserializeObject<User>(record.Body);

                    context.Logger.Log("About to call Cognito to create user");

                    await GetUserService().CreateUser(user);

                    context.Logger.Log("Just called Cognito to create user");
                }
            }
            catch (Exception e) {
                context.Logger.Log(e.Message);
                context.Logger.Log(e.StackTrace);

                if (e.InnerException != null)
                {
                    context.Logger.Log(e.InnerException.Message);
                    context.Logger.Log(e.InnerException.StackTrace);
                }
            }
        }

        private IUserService GetUserService()
        {
            return serviceProvider.GetRequiredService<IUserService>();
        }
    }
}
