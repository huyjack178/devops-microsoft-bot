using System;
using Fanex.Bot.Core._Shared.Enumerations;
using Newtonsoft.Json;

namespace Fanex.Bot.Core.GitLab.Models.JobEvents
{
    public class JobEvent : GitlabEvent
    {
        [JsonProperty("build_id")]
        public string BuildId { get; set; }

        [JsonProperty("build_name")]
        public string BuildName { get; set; }

        [JsonProperty("build_stage")]
        public string BuildStage { get; set; }

        [JsonProperty("build_status")]
        public string BuildStatus { get; set; }

        public JobStatus Status => Enumeration.FromDisplayName<JobStatus>(BuildStatus);

        [JsonProperty("build_started_at")]
        public string BuildStartAt { get; set; }

        [JsonProperty("build_finished_at")]
        public string BuildFinishedAt { get; set; }

        [JsonProperty("build_duration")]
        public string BuildDuration { get; set; }

        [JsonProperty("build_failure_reason")]
        public string BuildFailureReason { get; set; }

        [JsonProperty("project_name")]
        public string ProjectName { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("repository")]
        public Project Project { get; set; }
    }
}