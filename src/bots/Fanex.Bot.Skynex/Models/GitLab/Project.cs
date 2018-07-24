namespace Fanex.Bot.Skynex.Models.GitLab
{
    using Newtonsoft.Json;

    public class Project
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("web_url")]
#pragma warning disable S3996 // URI properties should not be strings
        public string WebUrl { get; set; }

        [JsonProperty("git_http_url")]
        public string GitHttpUrl { get; set; }

#pragma warning restore S3996 // URI properties should not be strings

        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; }
    }
}