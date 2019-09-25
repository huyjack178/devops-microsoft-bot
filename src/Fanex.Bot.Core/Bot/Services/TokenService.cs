using System;
using System.Threading.Tasks;
using Fanex.Bot.Core.Bot.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace Fanex.Bot.Core.Bot.Services
{
    public interface ITokenService
    {
        Task<string> GetToken(string clientId, string clientPassword);
    }

    public class TokenService : ITokenService
    {
        private const string DefaultClientPassword = "Bult8TMrOurQs1JtkvbvCBjJypuSmNO8";
        private readonly IConfiguration configuration;
        private readonly IRestClient restClient;

        public TokenService(IConfiguration configuration, IRestClient restClient)
        {
            this.configuration = configuration;
            this.restClient = restClient;
        }

        public async Task<string> GetToken(string clientId, string clientPassword)
        {
            if (clientPassword != DefaultClientPassword)
            {
                return null;
            }

            restClient.BaseUrl = new Uri(configuration.GetSection("TokenUrl").Value);

            var request = new RestRequest(Method.POST);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", configuration.GetSection("MicrosoftAppId").Value);
            request.AddParameter("client_secret", configuration.GetSection("MicrosoftAppPassword").Value);
            request.AddParameter("scope", $"{configuration.GetSection("MicrosoftAppId").Value}/.default");
            request.AddHeader("Content-type", "application/x-www-form-urlencoded");
            var response = await restClient.ExecuteTaskAsync<Token>(request).ConfigureAwait(false);

            return response.Data.AccessToken;
        }
    }
}