namespace Fanex.Bot.Skynex.Services
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Web;
    using Microsoft.Extensions.Configuration;

    public interface IUMService
    {
        Task<bool> CheckUM();
    }

    public class UMService : IUMService
    {
        private readonly IWebClient _webClient;
        private readonly string _mSiteUrl;

        public UMService(IWebClient webClient, IConfiguration configuration)
        {
            _webClient = webClient;
            _mSiteUrl = configuration.GetSection("LogInfo")?.GetSection("mSiteUrl")?.Value;
        }

        public async Task<bool> CheckUM()
        {
            var isUM = await _webClient.GetJsonAsync<bool>(new Uri($"{_mSiteUrl}/Bot/CheckUM"));

            return isUM;
        }
    }
}