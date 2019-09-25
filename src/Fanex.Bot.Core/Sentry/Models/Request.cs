using Newtonsoft.Json;

namespace Fanex.Bot.Core.Sentry.Models
{
    public class Request
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
    }
}