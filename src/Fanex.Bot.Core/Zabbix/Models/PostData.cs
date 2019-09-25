using Newtonsoft.Json;

namespace Fanex.Bot.Core.Zabbix.Models
{
    public class PostData
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public object Params { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; } = 1;

        [JsonProperty("auth")]
        public string Auth { get; set; }
    }
}