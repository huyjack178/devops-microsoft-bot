namespace Fanex.Bot.Letstalk.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Produces("application/json")]
    [Route("api/Error")]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                var exceptionThatOccurred = exceptionFeature.Error;
                _logger.LogError(
                    $"{exceptionThatOccurred}\n{exceptionThatOccurred.StackTrace}",
                    "Stopped program because of exception");

                return Forbid();
            }

            return Ok();
        }
    }
}