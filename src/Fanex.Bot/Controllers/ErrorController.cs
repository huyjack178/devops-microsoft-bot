namespace Fanex.Bot.Controllers
{
    using Fanex.Bot.Dialogs;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NLog.Web;

    [Produces("application/json")]
    [Route("api/Error")]
    public class ErrorController : Controller
    {
        private readonly IDialog _dialog;
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(IDialog dialog, ILogger<ErrorController> logger)
        {
            _dialog = dialog;
            _logger = logger;
        }

        public ActionResult Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                var exceptionThatOccurred = exceptionFeature.Error;
                _logger.LogError(exceptionThatOccurred, "Stopped program because of exception");

                _dialog.SendAdminAsync(exceptionThatOccurred.ToString());
            }

            return Ok();
        }
    }
}