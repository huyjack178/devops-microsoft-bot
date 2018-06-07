namespace Fanex.Bot.Dialogs.Impl
{
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.GitLab;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class GitLabDialog : Dialog, IGitLabDialog
    {
        private const string MasterBranchName = "heads/master";
        private readonly BotDbContext _dbContext;

        public GitLabDialog(
           IConfiguration configuration,
           BotDbContext dbContext)
           : base(configuration, dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task HandleMessageAsync(Activity activity, string message)
        {
            if (message.StartsWith("gitlab addproject"))
            {
                await AddProjectAsync(activity, message);
            }
            else
            {
                await SendAsync(activity, GetCommandMessages());
            }
        }

        private async Task AddProjectAsync(Activity activity, string message)
        {
            var projectUrl = message.Replace("gitlab addproject", string.Empty).Trim();

            if (string.IsNullOrEmpty(projectUrl))
            {
                await SendAsync(activity, "Please input project url");
                return;
            }

            var gitLabInfo = GetGitLabInfo(activity, projectUrl);

            await SaveGitLabInfoAsync(gitLabInfo);
            await SendAsync(activity, $"You will receive notification of project **{projectUrl}**");
        }

        private async Task SaveGitLabInfoAsync(GitLabInfo gitLabInfo)
        {
            bool existInfo = _dbContext.GitLabInfo.Any(e =>
                   e.ConversationId == gitLabInfo.ConversationId &&
                   e.ProjectUrl == gitLabInfo.ProjectUrl);

            _dbContext.Entry(gitLabInfo).State = existInfo ? EntityState.Modified : EntityState.Added;

            await _dbContext.SaveChangesAsync();
        }

#pragma warning disable S3994 // URI Parameters should not be strings

        private GitLabInfo GetGitLabInfo(Activity activity, string projectUrl)
        {
            var gitLabInfo = _dbContext.GitLabInfo.FirstOrDefault(
                info =>
                    info.ConversationId == activity.Conversation.Id &&
                    info.ProjectUrl.Contains(projectUrl.ToLowerInvariant()));

            if (gitLabInfo == null)
            {
                gitLabInfo = new GitLabInfo
                {
                    ConversationId = activity.Conversation.Id,
                    ProjectUrl = projectUrl
                };
            }

            return gitLabInfo;
        }

#pragma warning restore S3994 // URI Parameters should not be strings

        public async Task HandlePushEventAsync(PushEvent pushEvent)
        {
            var project = pushEvent.Project;
            var commits = pushEvent.Commits;
            var branchName = pushEvent.Ref.ToLowerInvariant();

            if (branchName.Contains(MasterBranchName))
            {
                var message = GeneratePushMasterMessage(project, commits);

                await SendEventMessageAsync(project, message);
            }
        }

        private static string GeneratePushMasterMessage(Project project, System.Collections.Generic.List<Commit> commits)
        {
            var message = $"**GitLab:** Master branch changes{Constants.NewLine}" +
                            $"**Repository:** {project.WebUrl}{Constants.NewLine}";

            var commitMessageBuilder = new StringBuilder();

            foreach (var commit in commits)
            {
                commitMessageBuilder.Append($"**Author:** {commit.Author.Name}{Constants.NewLine}");
                commitMessageBuilder.Append($"**Message:** {commit.Message}{Constants.NewLine}");
                commitMessageBuilder.Append($"--------------{Constants.NewLine}");
            }

            message += commitMessageBuilder;

            return message;
        }

        private async Task SendEventMessageAsync(Project project, string message)
        {
            var projectUrl = project.WebUrl.ToLowerInvariant()
                .Replace("http://", string.Empty)
                .Replace("https://", string.Empty);

            var gitlabInfos = _dbContext.GitLabInfo.Where(
                info => info.ProjectUrl.Contains(projectUrl));

            foreach (var gitlabInfo in gitlabInfos)
            {
                await SendAsync(gitlabInfo.ConversationId, message);
            }
        }
    }
}