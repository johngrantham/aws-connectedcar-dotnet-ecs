using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Amazon.DynamoDBv2;
using ConnectedCar.Core.Shared.AuthPolicy;
using ConnectedCar.Core.Shared.Services;
using ConnectedCar.Core.Services;
using ConnectedCar.Core.Services.Context;
using ConnectedCar.Core.Services.Translator;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ConnectedCar.Lambda
{
    public class VehicleAuthorizer
    {
        private const string HeaderXVin = "X-Vin";
        private const string HeaderXPin = "X-Pin";

        private readonly ServiceProvider serviceProvider;

        static void initialize()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
        }

        public VehicleAuthorizer(ServiceProvider serviceProvider) {
            this.serviceProvider = serviceProvider;
        }

        public VehicleAuthorizer()
        {
            AWSSDKHandler.RegisterXRay<IAmazonDynamoDB>();

            var services = new ServiceCollection();

            services.AddSingleton<IServiceContext, CloudServiceContext>();
            services.AddSingleton<ITranslator, Translator>();
            services.AddSingleton<IVehicleService, VehicleService>();

            serviceProvider = services.BuildServiceProvider();
        }

        public async Task<AuthPolicy> Authorize(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                if (request.Headers.ContainsKey(HeaderXVin) && request.Headers.ContainsKey(HeaderXPin))
                {
                    string vin = request.Headers[HeaderXVin];
                    string pin = request.Headers[HeaderXPin];

                    bool isAllowed = await GetVehicleService().ValidatePin(vin, pin);

                    return AuthPolicyFactory.GetApiPolicy(
                        vin,
                        isAllowed);
                }

                throw new InvalidOperationException();
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

            return null;
        }

        private IVehicleService GetVehicleService()
        {
            return serviceProvider.GetRequiredService<IVehicleService>();
        }
    }
}
