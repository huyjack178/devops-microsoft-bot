namespace Fanex.Bot.Models.Sentry
{
    using Newtonsoft.Json;

    public class Event
    {
        [JsonProperty("received")]
        public string LogTime { get; set; }

        [JsonProperty("sentry.interfaces.User")]
        public User User { get; set; }

        [JsonProperty("sentry.interfaces.Message")]
        public Message Message { get; set; }
    }
}