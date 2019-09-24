namespace Fanex.Bot.Core.Zabbix.Models
{
    public class RequestGetServices
    {
        public string[] ServiceKeys { get; set; }

        public string[] Hosts { get; set; }
    }
}