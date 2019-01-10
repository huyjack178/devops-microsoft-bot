namespace Fanex.Bot.Models.Zabbix
{
    using Newtonsoft.Json;

    public class Interface
    {
        [JsonProperty("ip")]
        public string IP { get; set; }
    }
}