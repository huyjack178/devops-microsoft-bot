namespace Fanex.Bot.Models.Zabbix
{
    using Newtonsoft.Json;

    public class Result<T>
    {
        [JsonProperty("result")]
        public T Content { get; set; }
    }
}