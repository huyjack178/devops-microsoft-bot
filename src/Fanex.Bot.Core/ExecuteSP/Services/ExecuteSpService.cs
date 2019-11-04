namespace Fanex.Bot.Core.ExecuteSP.Services
{
    using System;
    using System.Net.Cache;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.ExecuteSP.Models;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using RestSharp;

    public class ExecuteSpService : IExecuteSpService
    {
        private readonly IRestClient restClient;
        private readonly string botServiceUrl;

        public ExecuteSpService(IRestClient restClient, IConfiguration configuration)
        {
            this.restClient = restClient;
            this.restClient.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            botServiceUrl = configuration.GetSection("BotServiceUrl")?.Value;
        }

        public async Task<ExecuteSpResult> ExecuteSpWithParams(string conversationId, string commands)
        {
            var result = new ExecuteSpResult { IsSuccessful = false };
            if (string.IsNullOrWhiteSpace(commands))
            {
                result.Message = "Syntax error. The Commands cannot be null";
            }
            else
            {
                var param = new ExecuteSpParam { ConversationId = conversationId, Command = commands };
                result = await ExecuteSPByWebClient(param).ConfigureAwait(false);
            }

            return result;
        }

        private async Task<ExecuteSpResult> ExecuteSPByWebClient(ExecuteSpParam param)
        {
            var result = new ExecuteSpResult();
            try
            {
                var response = await ExecutePostAsync(new Uri($"{botServiceUrl}/ExecuteSp/Execute"), param)
                    .ConfigureAwait(false);

                result = JsonConvert.DeserializeObject<ExecuteSpResult>(response.Content);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<IRestResponse> ExecutePostAsync<T>(Uri url, T data)
        {
            restClient.BaseUrl = url;
            var jsonData = JsonConvert.SerializeObject(data);
            var request = new RestRequest(Method.POST);
            request.AddParameter("application/json", jsonData, ParameterType.RequestBody);

            var response = await restClient.ExecuteTaskAsync(request).ConfigureAwait(false);

            return response;
        }
    }
}