using Newtonsoft.Json;

namespace Fanex.Bot.Core.GitLab.Models.JobEvents
{
    public class Project
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("homepage")]
        public string Url { get; set; }
    }
}