using Newtonsoft.Json;

namespace Fanex.Bot.Core.GitLab.Models.JobEvents
{
    public class User
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}