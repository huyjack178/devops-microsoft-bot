namespace Fanex.Bot.Controllers
{
    using Fanex.Bot.Dialogs;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Produces("application/json")]
    [Route("api/Error")]
    public class ErrorController : Controller
    {
        private readonly IDialog _dialog;

        public ErrorController(IDialog dialog)
        {
            _dialog = dialog;
        }

        public ActionResult Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                var exceptionThatOccurred = exceptionFeature.Error;
                _dialog.SendAdminAsync(exceptionThatOccurred.ToString());
            }

            return Ok();
        }
    }
}