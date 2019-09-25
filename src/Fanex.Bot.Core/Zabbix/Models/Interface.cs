using Newtonsoft.Json;

namespace Fanex.Bot.Core.Zabbix.Models
{
    public class Interface
    {
        [JsonProperty("ip")]
        public string IP { get; set; }
    }
}