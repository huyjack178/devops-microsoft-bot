using Newtonsoft.Json;

namespace Fanex.Bot.Core.Sentry.Models
{
    public class Event
    {
        [JsonProperty("event_id")]
        public string Id { get; set; }

        [JsonProperty("timestamp")]
        public string LogTime { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("title")]
        public string Message { get; set; }

        [JsonProperty("culprit")]
        public string Culprit { get; set; }

        [JsonProperty("environment")]
        public string Environment { get; set; }

        [JsonProperty("request")]
        public Request Request { get; set; }

        [JsonProperty("contexts")]
        public Context Context { get; set; }
    }
}