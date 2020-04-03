using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.GitLab.Enumerations;
using Newtonsoft.Json;

namespace Fanex.Bot.Core.GitLab.Models
{
    public class GitlabEvent
    {
        [JsonProperty("object_kind")]
        public string ObjectKind { get; set; }

        public GitlabEventTypes EventType => Enumeration.FromDisplayName<GitlabEventTypes>(ObjectKind);
    }
}