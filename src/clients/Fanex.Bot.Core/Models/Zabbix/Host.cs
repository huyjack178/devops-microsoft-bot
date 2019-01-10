namespace Fanex.Bot.Models.Zabbix
{
    using Newtonsoft.Json;

    public class Host
    {
        [JsonProperty("hostid")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}