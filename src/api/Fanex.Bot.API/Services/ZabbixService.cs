using Fanex.Bot.Core.Zabbix.Models;

namespace Fanex.Bot.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Common.Helpers.Web;
    using Microsoft.Extensions.Configuration;

    public interface IZabbixService
    {
        Task<string> Login();

        Task<IList<Service>> GetServices(string[] serviceKeys, string[] hosts);
    }

    public class ZabbixService : IZabbixService
    {
        private readonly IConfiguration configuration;
        private readonly IWebClient webClient;

        public ZabbixService(IConfiguration configuration, IWebClient webClient)
        {
            this.configuration = configuration;
            this.webClient = webClient;
        }

        public async Task<IList<Service>> GetServices(string[] serviceKeys, string[] hosts)
        {
            var url = new Uri(configuration.GetSection("Zabbix:ServiceUri").Value);
            var token = await Login();

            var services = await webClient.PostJsonAsync<PostData, Result<List<Service>>>(url, new PostData
            {
                Method = "item.get",
                Params = new
                {
                    output = new[] { "itemid", "type", "name", "key_", "status", "lastvalue" },
                    search = new
                    {
                        key_ = serviceKeys
                    },
                    searchByAny = "True",
                    selectHosts = new[] { "name" },
                    selectInterfaces = new[] { "ip" }
                },
                Auth = token
            });

            return services.Content
                .Where(service =>
                    service.Interfaces.Count > 0 &&
                    service.Status == "0" &&
                    service.Interfaces.Any(i => hosts == null || hosts.Length == 0 || hosts.Contains(i.IP))).ToList();
        }

        public async Task<string> Login()
        {
            var url = new Uri(configuration.GetSection("Zabbix:ServiceUri").Value);
            var token = await webClient.PostJsonAsync<PostData, Result<string>>(url, new PostData
            {
                Method = "user.login",
                Params = new
                {
                    user = configuration.GetSection("Zabbix:UserName").Value
                            ?? throw new MissingFieldException("Missing configuration for Zabbix:UserName"),
                    password = configuration.GetSection("Zabbix:Password").Value
                            ?? throw new MissingFieldException("Missing configuration for Zabbix:Password"),
                }
            });

            return token.Content;
        }
    }
}