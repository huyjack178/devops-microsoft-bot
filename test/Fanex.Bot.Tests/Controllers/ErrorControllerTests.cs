namespace Fanex.Bot.Skynex.Tests.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Controllers;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Xunit;

    public class ErrorControllerTests
    {
        private readonly IConversation _conversation;
        private readonly ILogger<ErrorController> _logger;

        public ErrorControllerTests()
        {
            _conversation = Substitute.For<IConversation>();
            _logger = Substitute.For<ILogger<ErrorController>>();
        }

        [Fact]
        public async Task Index_HasException_HandleException()
        {
            // Arrange
            var expectedException = new ExceptionHandlerFeature()
            {
                Error = new Exception { Source = "exception" }
            };

            var controller = new ErrorController(_conversation, _logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            controller.ControllerContext.HttpContext.Features.Set<IExceptionHandlerPathFeature>(expectedException);

            // Act
            var result = await controller.Index();

            // Assert
            await _conversation.Received().SendAdminAsync(
                $"**Exception occured in Skynex** {Constants.NewLine}" +
                    $"Exception of type 'System.Exception' was thrown. {Constants.NewLine}");
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Index_NoException_HandleException()
        {
            // Arrange
            var controller = new ErrorController(_conversation, _logger)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = await controller.Index();

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }
}