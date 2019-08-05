namespace Fanex.Bot.Models.Sentry
{
    using Newtonsoft.Json;

    public class Event
    {
        [JsonProperty("event_id")]
        public string Id { get; set; }

        [JsonProperty("timestamp")]
        public string LogTime { get; set; }

        [JsonProperty("sentry.interfaces.User")]
        public User User { get; set; }

        [JsonProperty("title")]
        public string Message { get; set; }
    }
}