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
        private readonly IConfiguration configuration;

        public ZabbixService(
           IWebClient webClient,
           IConfiguration configuration)
        {
            this.webClient = webClient;
            botServiceUrl = configuration.GetSection("BotServiceUrl")?.Value;
            this.configuration = configuration;
        }

        public async Task<IList<Service>> GetServices()
        {
            var zabbixSearchServiceKeys = configuration.GetSection("Zabbix:SearchServiceKeys")?.Get<string[]>();
            var hosts = configuration.GetSection("Zabbix:Hosts")?.Get<string[]>();

            var services = await webClient.PostJsonAsync<RequestGetServices, IList<Service>>(
                    new Uri($"{botServiceUrl}/Zabbix/Services"),
                    new RequestGetServices
                    {
                        ServiceKeys = zabbixSearchServiceKeys,
                        Hosts = hosts
                    }).ConfigureAwait(false);

            return services;
        }
    }
}