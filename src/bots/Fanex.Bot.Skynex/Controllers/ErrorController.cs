namespace Fanex.Bot.Skynex.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Produces("application/json")]
    [Route("api/Error")]
    public class ErrorController : Controller
    {
        private readonly IConversation conversation;
        private readonly ILogger<ErrorController> logger;

        public ErrorController(IConversation conversation, ILogger<ErrorController> logger)
        {
            this.conversation = conversation;
            this.logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                var exceptionThatOccurred = exceptionFeature.Error;
                logger.LogError(
                    $"{exceptionThatOccurred.Message}\n{exceptionThatOccurred.StackTrace}\n");

                await conversation.SendAdminAsync(
                    $"**Exception occured in Skynex** {MessageFormatSignal.NewLine}" +
                    $"{exceptionThatOccurred.Message} {MessageFormatSignal.NewLine}" +
                    $"{exceptionThatOccurred.StackTrace}");

                return Forbid();
            }

            return Ok();
        }
    }
}