using System;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core.GitLab.Models;
using Fanex.Bot.Core.Utilities.Bot;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.Bot.Connector;
using Microsoft.EntityFrameworkCore;

namespace Fanex.Bot.Skynex.GitLab
{
    public interface IGitLabDialog : IDialog
    {
        Task HandlePushEventAsync(PushEvent pushEvent);
    }

    public class GitLabDialog : BaseDialog, IGitLabDialog
    {
        private const string MasterBranchName = "heads/master";
        private const string RemoveProjectCmd = "gitlab removeproject";
        private const string AddProjectCmd = "gitlab addproject";
        private readonly IGitLabMessageBuilder gitLabMessageBuilder;

        public GitLabDialog(
           BotDbContext dbContext,
           IConversation conversation,
           IGitLabMessageBuilder gitLabMessageBuilder)
           : base(dbContext, conversation)
        {
            this.gitLabMessageBuilder = gitLabMessageBuilder;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            if (message.StartsWith(AddProjectCmd))
            {
                await AddProjectAsync(activity, message);
            }
            else if (message.StartsWith(RemoveProjectCmd))
            {
                await DisableProjectAsync(activity, message);
            }
            else
            {
                await Conversation.ReplyAsync(activity, GetCommandMessages());
            }
        }

        private async Task AddProjectAsync(IMessageActivity activity, string message)
        {
            var projectUrl = ExtractProjectLink(message.Replace(AddProjectCmd, string.Empty).Trim());

            if (string.IsNullOrEmpty(projectUrl))
            {
                await Conversation.ReplyAsync(activity, "Please input project url");
                return;
            }

            var gitLabInfo = await GetGitLabInfo(activity, projectUrl);
            gitLabInfo.IsActive = true;

            await SaveGitLabInfoAsync(gitLabInfo);
            await Conversation.ReplyAsync(activity, $"You will receive notification of project {MessageFormatSymbol.BOLD_START}{projectUrl}{MessageFormatSymbol.BOLD_END}");
        }

        private async Task DisableProjectAsync(IMessageActivity activity, string message)
        {
            var projectUrl = ExtractProjectLink(message.Replace(RemoveProjectCmd, string.Empty).Trim());

            if (string.IsNullOrEmpty(projectUrl))
            {
                await Conversation.ReplyAsync(activity, "Please input project url");
                return;
            }

            var gitLabInfo = await GetExistingGitLabInfo(activity, projectUrl);

            if (gitLabInfo == null)
            {
                await Conversation.ReplyAsync(activity, "Project not found");
                return;
            }

            gitLabInfo.IsActive = false;
            await SaveGitLabInfoAsync(gitLabInfo);
            await Conversation.ReplyAsync(activity, $"You will not receive notification of project {MessageFormatSymbol.BOLD_START}{projectUrl}{MessageFormatSymbol.BOLD_END}");
        }

        private async Task SaveGitLabInfoAsync(GitLabInfo gitLabInfo)
        {
            bool existInfo = DbContext.GitLabInfo.AsNoTracking().Any(e =>
                   e.ConversationId == gitLabInfo.ConversationId &&
                   e.ProjectUrl == gitLabInfo.ProjectUrl);

            DbContext.Entry(gitLabInfo).State = existInfo ? EntityState.Modified : EntityState.Added;

            await DbContext.SaveChangesAsync();
        }

#pragma warning disable S3994 // URI Parameters should not be strings

        private async Task<GitLabInfo> GetGitLabInfo(IMessageActivity activity, string projectUrl)
        {
            var gitLabInfo = await GetExistingGitLabInfo(activity, projectUrl);

            if (gitLabInfo == null)
            {
                gitLabInfo = new GitLabInfo
                {
                    ConversationId = activity.Conversation.Id,
                    ProjectUrl = projectUrl,
#pragma warning disable S109 // Magic numbers should not be used
                    CreatedTime = DateTime.UtcNow.AddHours(7)
#pragma warning restore S109 // Magic numbers should not be used
                };
            }

            return gitLabInfo;
        }

        private Task<GitLabInfo> GetExistingGitLabInfo(IMessageActivity activity, string formatedProjectUrl)
            => DbContext.GitLabInfo
                .AsNoTracking()
                .FirstOrDefaultAsync(info =>
                    info.ConversationId == activity.Conversation.Id &&
                    formatedProjectUrl.Contains(info.ProjectUrl));

        public async Task HandlePushEventAsync(PushEvent pushEvent)
        {
            var project = pushEvent.Project;
            var branchName = pushEvent.Ref?.ToLowerInvariant() ?? string.Empty;

            if (branchName.Contains(MasterBranchName))
            {
                var message = gitLabMessageBuilder.BuildMessage(pushEvent);

                await SendEventMessageAsync(project, message);
            }
        }

        private async Task SendEventMessageAsync(Project project, string message)
        {
            var projectUrl = project.WebUrl.ToLowerInvariant()
                .Replace("http://", string.Empty)
                .Replace("https://", string.Empty);

            var gitlabInfos = DbContext.GitLabInfo.Where(
                    info => projectUrl.Contains(info.ProjectUrl) &&
                    info.IsActive);

            foreach (var gitlabInfo in gitlabInfos)
            {
                await Conversation.SendAsync(gitlabInfo.ConversationId, message);
            }
        }

        private static string ExtractProjectLink(string projectUrl)
        {
            string formatedProjectUrl = BotHelper.ExtractProjectLink(projectUrl);

            return formatedProjectUrl
                    .Replace("http://", string.Empty)
                    .Replace("https://", string.Empty);
        }
    }

#pragma warning restore S3994 // URI Parameters should not be strings
}