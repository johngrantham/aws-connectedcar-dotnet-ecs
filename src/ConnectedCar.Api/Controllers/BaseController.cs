using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;

namespace ConnectedCar.Api.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        private readonly ILogger logger;

        public BaseController(ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger("Controller");            
        }

        protected async Task<ActionResult> Process(Func<Task<ActionResult>> func)
        {
            try
            {
                return await func.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                return BadRequest();
            }
        }
    }
}