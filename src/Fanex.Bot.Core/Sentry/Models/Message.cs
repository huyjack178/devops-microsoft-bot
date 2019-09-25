using Newtonsoft.Json;

namespace Fanex.Bot.Core.Sentry.Models
{
    public class Message
    {
        [JsonProperty("message")]
        public string MessageInfo { get; set; }
    }
}