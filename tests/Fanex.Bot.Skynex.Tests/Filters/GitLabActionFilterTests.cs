namespace Fanex.Bot.Skynex.Tests.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Filters;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;
    using Xunit;

    public class GitLabActionFilterTests
    {
        private readonly IConfiguration _configuration;

        public GitLabActionFilterTests()
        {
            _configuration = Substitute.For<IConfiguration>();
        }

        [Fact]
        public void OnActionExecuted_Always_ReturnOkResult()
        {
            // Arrange
            var filters = Substitute.For<IList<IFilterMetadata>>();
            var attribute = new GitLabAttribute(_configuration);
            var context = new ActionExecutedContext(new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                ActionDescriptor = new ActionDescriptor(),
                RouteData = new RouteData()
            }, filters, null);

            // Act
            attribute.OnActionExecuted(context);

            // Assert
            Assert.Equal(context.Result.ToString(), new OkResult().ToString());
        }

        [Fact]
        public void OnActionExecuted_InvalidGitLabToken_ReturnUnauthorizedResult()
        {
            // Arrange
            var filters = Substitute.For<IList<IFilterMetadata>>();
            var attribute = new GitLabAttribute(_configuration);
            var context = new ActionExecutingContext(new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                ActionDescriptor = new ActionDescriptor(),
                RouteData = new RouteData()
            }, filters, new Dictionary<string, object>(), null);

            context.HttpContext.Request.Headers.Add("X-Gitlab-Token", "12345");
            _configuration.GetSection("GitLabInfo")?.GetSection("SecretToken")?.Value.Returns("123456");

            // Act
            attribute.OnActionExecuting(context);

            // Assert
            Assert.Equal(context.Result.ToString(), new UnauthorizedResult().ToString());
        }
    }
}