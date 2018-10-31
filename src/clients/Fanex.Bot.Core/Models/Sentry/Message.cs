namespace Fanex.Bot.Models.Sentry
{
    using Newtonsoft.Json;

    public class Message
    {
        [JsonProperty("message")]
        public string MessageInfo { get; set; }
    }
}