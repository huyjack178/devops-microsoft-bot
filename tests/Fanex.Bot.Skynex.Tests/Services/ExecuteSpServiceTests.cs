namespace Fanex.Bot.Skynex.Tests.Services
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.ExecuteSP.Models;
    using Fanex.Bot.Core.ExecuteSP.Services;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using RestSharp;
    using Xunit;

    public class ExecuteSpServiceTests
    {
        private readonly IRestClient restClient;
        private readonly IConfiguration configuration;
        private readonly IExecuteSpService executeSpService;

        public ExecuteSpServiceTests()
        {
            configuration = Substitute.For<IConfiguration>();
            configuration.GetSection("BotServiceUrl")?.Value.Returns("localhost:6969/api");
            restClient = Substitute.For<IRestClient>();
            executeSpService = new ExecuteSpService(restClient, configuration);
        }

        [Fact]
        public async Task ExecuteSpWithParams_WhenIncorrectSyntax_ReturnExpectedResult()
        {
            string message = "";
            var expect = new ExecuteSpResult
            {
                IsSuccessful = false,
                Message = "Syntax error. The Commands cannot be null"
            };

            var actual = await executeSpService.ExecuteSpWithParams("conversationId", message);

            Assert.Equal(JsonConvert.SerializeObject(expect), JsonConvert.SerializeObject(actual));
        }

        [Fact]
        public async Task ExecuteSpWithParams_WhenCorrectSyntax_NotThrowsException()
        {
            string message = "query commands";
            var expect = new ExecuteSpResult
            {
                IsSuccessful = true,
                Message = "Success"
            };
            var response = new RestResponse { Content = JsonConvert.SerializeObject(expect), StatusCode = HttpStatusCode.OK };
            restClient.ExecuteTaskAsync(Arg.Any<RestRequest>()).Returns(response);
            var actual = await executeSpService.ExecuteSpWithParams("conversationId", message);

            Assert.Equal(JsonConvert.SerializeObject(expect), JsonConvert.SerializeObject(actual));
        }

        [Fact]
        public async Task ExecuteSpWithParams_WhenCorrectSyntax_ThrowsException()
        {
            string message = "query commands";
            var expect = "exception message";
            restClient.ExecuteTaskAsync(Arg.Any<RestRequest>()).Throws(new Exception(expect));
            var actual = await executeSpService.ExecuteSpWithParams("conversationId", message);

            Assert.Equal(expect, actual.Message);
        }
    }
}