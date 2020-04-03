using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Common.Helpers.Bot;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core.GitLab.Models;
using Fanex.Bot.Core.GitLab.Models.JobEvents;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.Bot.Connector;
using Microsoft.EntityFrameworkCore;

namespace Fanex.Bot.Skynex.GitLab
{
    public interface IGitLabDialog : IDialog
    {
        Task HandlePushEventAsync(PushEvent pushEvent);

        Task HandleJobEventAsync(JobEvent jobEvent);
    }

    public class GitLabDialog : BaseDialog, IGitLabDialog
    {
        private const string MasterBranchName = "heads/master";
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
            var messageParts = message.Split(" ");

            if (messageParts.Length < 2)
            {
                await Conversation.ReplyAsync(activity, GetCommandMessages());
                return;
            }

            var command = messageParts[1].ToLowerInvariant();

            if (command == GitlabCommand.AddProject && messageParts.Length > 2)
            {
                await AddProjectAsync(activity, messageParts[2]);
                return;
            }
            if (command == GitlabCommand.RemoveProject && messageParts.Length > 2)
            {
                await DisableProjectAsync(activity, messageParts[2]);
                return;
            }

            if ((command == GitlabCommand.Enable || command == GitlabCommand.Disable) && messageParts.Length > 2)
            {
                var function = messageParts[2];
                await EnableDisableFunction(activity, function, command == GitlabCommand.Enable);
            }

            await Conversation.ReplyAsync(activity, "Please input right command!");
        }

        private async Task AddProjectAsync(IMessageActivity activity, string repo)
        {
            var projectUrl = ExtractProjectLink(repo).Trim();

            var gitLabInfo = await GetGitLabInfo(activity, projectUrl);
            gitLabInfo.IsActive = true;
            gitLabInfo.EnablePush = true;
            gitLabInfo.EnablePipeline = true;

            await SaveGitLabInfoAsync(gitLabInfo);
            await Conversation.ReplyAsync(activity,
                $"You will receive notification of project {MessageFormatSymbol.BOLD_START}{projectUrl}{MessageFormatSymbol.BOLD_END}");
        }

        private async Task DisableProjectAsync(IMessageActivity activity, string repo)
        {
            var projectUrl = ExtractProjectLink(repo).Trim();
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

        private async Task EnableDisableFunction(IMessageActivity activity, string functionName, bool isEnabled)
        {
            foreach (var gitlabInfo in GetExistingGitLabInfo(activity))
            {
                if (functionName == GitlabCommand.PushEvent)
                {
                    gitlabInfo.EnablePush = isEnabled;
                }
                else if (functionName == GitlabCommand.JobEvent)
                {
                    gitlabInfo.EnablePipeline = isEnabled;
                }

                await SaveGitLabInfoAsync(gitlabInfo);
            }

            await Conversation.ReplyAsync(activity, "Your request is completed!");
        }

        public async Task HandlePushEventAsync(PushEvent pushEvent)
        {
            var project = pushEvent.Project;
            var branchName = pushEvent.Ref?.ToLowerInvariant() ?? string.Empty;

            if (branchName.Contains(MasterBranchName))
            {
                var message = gitLabMessageBuilder.BuildMessage(pushEvent);

                await SendEventMessageAsync(project.WebUrl, message, GitlabCommand.PushEvent);
            }
        }

        public async Task HandleJobEventAsync(JobEvent jobEvent)
        {
            var project = jobEvent.Project;
            var message = gitLabMessageBuilder.BuildJobMessage(jobEvent);

            await SendEventMessageAsync(project.Url, message, GitlabCommand.JobEvent);
        }

        private async Task SendEventMessageAsync(string projectWebUrl, string message, string functionName)
        {
            var projectUrl = projectWebUrl.ToLowerInvariant()
                .Replace("http://", string.Empty)
                .Replace("https://", string.Empty);

            var gitlabInfos = DbContext.GitLabInfo.Where(
                    info => projectUrl.Contains(info.ProjectUrl) &&
                    info.IsActive &&
                    ((info.EnablePush && functionName == GitlabCommand.PushEvent)
                        || (info.EnablePipeline && functionName == GitlabCommand.JobEvent)));

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

        private IEnumerable<GitLabInfo> GetExistingGitLabInfo(IMessageActivity activity)
          => DbContext.GitLabInfo
              .AsNoTracking()
              .Where(info => info.ConversationId == activity.Conversation.Id);
    }
}

#pragma warning restore S3994 // URI Parameters should not be strings