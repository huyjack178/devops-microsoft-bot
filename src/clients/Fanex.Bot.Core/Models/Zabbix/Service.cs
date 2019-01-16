namespace Fanex.Bot.Models.Zabbix
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Service
    {
        [JsonProperty("itemid")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("key_")]
        public string Key { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("lastvalue")]
        public string LastValue { get; set; }

        [JsonProperty("hosts")]
        public IList<Host> Hosts { get; set; }

        [JsonProperty("interfaces")]
        public IList<Interface> Interfaces { get; set; }
    }
}