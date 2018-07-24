﻿namespace Fanex.Bot.Skynex.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Utilities.Bot;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Produces("application/json")]
    [Route("api/Error")]
    public class ErrorController : Controller
    {
        private readonly IConversation _conversation;
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(IConversation conversation, ILogger<ErrorController> logger)
        {
            _conversation = conversation;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                var exceptionThatOccurred = exceptionFeature.Error;
                _logger.LogError(
                    $"{exceptionThatOccurred.Message}\n{exceptionThatOccurred.StackTrace}\n");

                await _conversation.SendAdminAsync(
                    $"**Exception occured in Skynex** {Constants.NewLine}" +
                    $"{exceptionThatOccurred.Message} {Constants.NewLine}" +
                    $"{exceptionThatOccurred.StackTrace}");

                return Forbid();
            }

            return Ok();
        }
    }
}