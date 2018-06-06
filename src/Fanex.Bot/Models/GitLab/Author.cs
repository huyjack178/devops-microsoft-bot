namespace Fanex.Bot.Models.GitLab
{
    using Newtonsoft.Json;

    public class Author
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}