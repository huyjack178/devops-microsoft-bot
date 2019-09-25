using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fanex.Bot.Core.GitLab.Models
{
    public class PushEvent
    {
        [JsonProperty("object_kind")]
        public string ObjectKind { get; set; }

        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }

        [JsonProperty("project")]
        public Project Project { get; set; }

        [JsonProperty("ref")]
        public string Ref { get; set; }

        [JsonProperty("commits")]
        public List<Commit> Commits { get; set; }

        [JsonProperty("total_commits_count")]
        public int TotalCommitsCount { get; set; }
    }
}