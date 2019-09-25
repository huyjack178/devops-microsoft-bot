using Newtonsoft.Json;

namespace Fanex.Bot.Core.GitLab.Models
{
    public class Author
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}