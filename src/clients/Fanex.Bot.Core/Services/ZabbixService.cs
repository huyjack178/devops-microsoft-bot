namespace Fanex.Bot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Models.Zabbix;
    using Microsoft.Extensions.Configuration;

    public interface IZabbixService
    {
        Task<IList<Service>> GetServices();
    }

    public class ZabbixService : IZabbixService
    {
        private readonly IWebClient webClient;
        private readonly string botServiceUrl;
        private readonly string[] zabbixSearchServiceKeys;

        public ZabbixService(
           IWebClient webClient,
           IConfiguration configuration)
        {
            this.webClient = webClient;
            botServiceUrl = configuration.GetSection("BotServiceUrl")?.Value;
            zabbixSearchServiceKeys = configuration.GetSection("Zabbix:SearchServiceKeys")?.Get<string[]>();
        }

        public async Task<IList<Service>> GetServices()
        {
            var services = await webClient.PostJsonAsync<string[], IList<Service>>(
                    new Uri($"{botServiceUrl}/Zabbix/Services"),
                    zabbixSearchServiceKeys).ConfigureAwait(false);

            return services;
        }
    }
}