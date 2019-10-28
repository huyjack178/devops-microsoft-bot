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

        public async Task<ExecuteSpResult> ExecuteSpWithParams(string message)
        {
            var result = new ExecuteSpResult { IsSuccessful = false };
            ExecuteSpParam param = ParseParamFromMessage(message);
            if (string.IsNullOrWhiteSpace(param.SpName) || string.IsNullOrWhiteSpace(param.GroupId) || string.IsNullOrWhiteSpace(param.Command))
            {
                result.Message = "Syntax error. The syntax is: execute_sp SpName(groupId, commands).";
            }
            else
            {
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

        private ExecuteSpParam ParseParamFromMessage(string message)
        {
            string spName = message.Substring(0, message.IndexOf('('));
            spName = spName.Replace("dbo.", string.Empty).Trim();
            var param = GetParamInsideMessage(message);
            var paramObject = param.Split(',');
            return new ExecuteSpParam
            {
                SpName = spName,
                GroupId = paramObject[0].Trim(),
                Command = paramObject[1].Trim()
            };
        }

        private static string GetParamInsideMessage(string message)
        {
            string param = string.Empty;
            int startIndex = message.IndexOf('(');
            int lastIndex = message.LastIndexOf(')');
            if (startIndex != -1 && lastIndex != -1)
            {
                param = message.Substring(startIndex + 1, message.Length - startIndex - 2);
            }

            return param;
        }
    }
}