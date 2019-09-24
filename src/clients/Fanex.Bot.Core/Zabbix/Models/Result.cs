using Newtonsoft.Json;

namespace Fanex.Bot.Core.Zabbix.Models
{
    public class Result<T>
    {
        [JsonProperty("result")]
        public T Content { get; set; }
    }
}