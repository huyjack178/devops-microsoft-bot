using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Common.Helpers.Web;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;

namespace Fanex.Bot.Core.UM.Services
{
    public interface IUnderMaintenanceService
    {
        Task<Dictionary<int, Models.UM>> GetScheduledInfo();

        Task<Dictionary<int, Models.UM>> GetActualInfo();

        Task<bool> CheckPageShowUM(Uri pageUrl);
    }

    public class UnderMaintenanceService : IUnderMaintenanceService
    {
        private readonly IWebClient webClient;
        private readonly string botServiceUrl;
        private readonly string[] underMaintenanceKeywords;

        public UnderMaintenanceService(IWebClient webClient, IConfiguration configuration)
        {
            this.webClient = webClient;
            botServiceUrl = configuration.GetSection("BotServiceUrl")?.Value;
            underMaintenanceKeywords = configuration.GetSection("UMInfo")?.GetSection("UMKeyWord").Get<string[]>();
        }

        public Task<Dictionary<int, Models.UM>> GetScheduledInfo()
            => webClient.GetJsonAsync<Dictionary<int, Models.UM>>(new Uri($"{botServiceUrl}/UnderMaintenance/ScheduledInfo"));

        public Task<Dictionary<int, Models.UM>> GetActualInfo()
           => webClient.GetJsonAsync<Dictionary<int, Models.UM>>(new Uri($"{botServiceUrl}/UnderMaintenance/ActualInfo"));

        public async Task<bool> CheckPageShowUM(Uri pageUrl)
        {
            try
            {
                var content = (await webClient.GetContentAsync(pageUrl).ConfigureAwait(false))
                    .ToLowerInvariant();
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);

                var titleNode = htmlDoc.DocumentNode
                        .Descendants()
                        .FirstOrDefault(node => node.Name == "title");
                var bodyNode = htmlDoc.DocumentNode
                        .Descendants()
                        .FirstOrDefault(node => node.Name == "body");

                return underMaintenanceKeywords.Any(word =>
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