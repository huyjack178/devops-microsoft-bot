using Newtonsoft.Json;

namespace Fanex.Bot.Core.Sentry.Models
{
    public class User
    {
        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}