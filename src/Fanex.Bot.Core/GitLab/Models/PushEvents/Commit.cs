using Newtonsoft.Json;

namespace Fanex.Bot.Core.GitLab.Models
{
    public class Commit
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }
    }
}