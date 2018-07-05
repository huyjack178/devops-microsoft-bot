namespace Fanex.Bot.Skynex.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Web;
    using HtmlAgilityPack;
    using Microsoft.Extensions.Configuration;

    public interface IUMService
    {
        Task<bool> CheckUM();

        Task<bool> CheckPageShowUM(Uri pageUrl);
    }

    public class UMService : IUMService
    {
        private readonly IWebClient _webClient;
        private readonly string _mSiteUrl;
        private readonly string[] _umKeywords;

        public UMService(IWebClient webClient, IConfiguration configuration)
        {
            _webClient = webClient;
            _mSiteUrl = configuration.GetSection("LogInfo")?.GetSection("mSiteUrl")?.Value;
            _umKeywords = configuration.GetSection("UMInfo")?.GetSection("UMKeyWord").Get<string[]>();
        }

        public async Task<bool> CheckUM()
        {
            var isUM = await _webClient.GetJsonAsync<bool>(new Uri($"{_mSiteUrl}/Bot/CheckUM"));

            return isUM;
        }

        public async Task<bool> CheckPageShowUM(Uri pageUrl)
        {
            var content =
                (await _webClient.GetContentAsync(pageUrl))
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
    }
}