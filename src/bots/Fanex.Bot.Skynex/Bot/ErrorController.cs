using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fanex.Bot.Skynex.Bot
{
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
                    $"**Exception occured in Skynex** {MessageFormatSignal.NEWLINE}" +
                    $"{exceptionThatOccurred.Message} {MessageFormatSignal.NEWLINE}" +
                    $"{exceptionThatOccurred.StackTrace}");

                return Forbid();
            }

            return Ok();
        }
    }
}