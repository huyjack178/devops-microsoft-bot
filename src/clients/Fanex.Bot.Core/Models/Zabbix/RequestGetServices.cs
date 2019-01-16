namespace Fanex.Bot.Models.Zabbix
{
    public class RequestGetServices
    {
        public string[] ServiceKeys { get; set; }

        public string[] Hosts { get; set; }
    }
}