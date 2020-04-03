using Fanex.Bot.Core._Shared.Enumerations;

namespace Fanex.Bot.Core.GitLab.Enumerations
{
    public class GitlabEventTypes : Enumeration
    {
        public const string PushType = "push";
        public const string JobType = "build";
        public const string PipelineType = "pipeline";
        public const string RepositoryUpdateType = "repository_update";

        public static readonly GitlabEventTypes Push = new GitlabEventTypes(1, PushType);

        public static readonly GitlabEventTypes Job = new GitlabEventTypes(2, JobType);

        public static readonly GitlabEventTypes Pipeline = new GitlabEventTypes(3, PipelineType);

        public static readonly GitlabEventTypes RepositoryUpdate = new GitlabEventTypes(4, RepositoryUpdateType);

        public GitlabEventTypes()
        {
        }

        private GitlabEventTypes(byte value, string displayName)
            : base(value, displayName)
        {
        }

        public bool IsPush => this == Push;

        public bool IsJob => this == Job;

        public bool IsPipeline => this == Pipeline;

        public bool IsRepositoryUpdate => this == RepositoryUpdate;
    }
}