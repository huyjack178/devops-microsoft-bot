using Newtonsoft.Json;

namespace Fanex.Bot.Core.Zabbix.Models
{
    public class Host
    {
        [JsonProperty("hostid")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}