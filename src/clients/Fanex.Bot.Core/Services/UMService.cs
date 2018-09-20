namespace Fanex.Bot.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Models.UM;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Configuration;

    public interface IUMService
    {
        Task<UM> GetUMInformation();

        Task<bool> CheckPageShowUM(Uri pageUrl);
    }

    public class UMService : IUMService
    {
        private readonly IWebClient _webClient;
        private readonly string _botServiceUrl;
        private readonly string[] _umKeywords;

        public UMService(IWebClient webClient, IConfiguration configuration)
        {
            _webClient = webClient;
            _botServiceUrl = configuration.GetSection("BotServiceUrl")?.Value;
            _umKeywords = configuration.GetSection("UMInfo")?.GetSection("UMKeyWord").Get<string[]>();
        }

        public Task<UM> GetUMInformation()
            => _webClient.GetJsonAsync<UM>(new Uri($"{_botServiceUrl}/UM/Information"));

        public async Task<bool> CheckPageShowUM(Uri pageUrl)
        {
            try
            {
                var content = (await _webClient.GetContentAsync(pageUrl).ConfigureAwait(false))
                    .ToLowerInvariant();
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);

                var titleNode = htmlDoc.DocumentNode
                        .Descendants()
                        .FirstOrDefault(node => node.Name == "title");
                var bodyNode = htmlDoc.DocumentNode
                        .Descendants()
                        .FirstOrDefault(node => node.Name == "body");

                return _umKeywords.Any(word =>
                            titleNode.InnerText.ToLowerInvariant().Contains(word) ||
                            bodyNode.InnerText.ToLowerInvariant().Contains(word));
            }
            catch
            {
                return false;
            }
        }
    }
}