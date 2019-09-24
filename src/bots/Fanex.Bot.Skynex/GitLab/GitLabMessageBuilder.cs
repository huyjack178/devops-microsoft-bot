using System.Text;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core.GitLab.Models;
using Fanex.Bot.Helpers;
using Fanex.Bot.Skynex.Log;

namespace Fanex.Bot.Skynex.GitLab
{
    public interface IGitLabMessageBuilder : IMessageBuilder
    {
    }

    public class GitLabMessageBuilder : IGitLabMessageBuilder
    {
        public string BuildMessage(object model)
        {
            var gitlabPushEvent = DataHelper.Parse<PushEvent>(model);
            var project = gitlabPushEvent.Project;
            var commits = gitlabPushEvent.Commits;

            var message = $"{MessageFormatSignal.BOLD_START}GitLab Master Branch Change{MessageFormatSignal.BOLD_END} (bell){MessageFormatSignal.NEWLINE}" +
                            $"{MessageFormatSignal.BOLD_START}Repository:{MessageFormatSignal.BOLD_END} {project.WebUrl}{MessageFormatSignal.NEWLINE}";

            var commitMessageBuilder = new StringBuilder();
            commitMessageBuilder.Append($"{MessageFormatSignal.BOLD_START}Commits:{MessageFormatSignal.BOLD_END}{MessageFormatSignal.NEWLINE}");

            foreach (var commit in commits)
            {
                var commitUrl = $"{project.WebUrl}/commit/{commit.Id}";

                commitMessageBuilder
                    .Append($"{MessageFormatSignal.BOLD_START}[{commit.Id.Substring(0, 8)}]({commitUrl}){MessageFormatSignal.BOLD_END}")
                    .Append($" {commit.Message} ({commit.Author.Name})")
                    .Append(MessageFormatSignal.NEWLINE);
            }

            commitMessageBuilder.Append(MessageFormatSignal.DIVIDER + MessageFormatSignal.NEWLINE);

            message += commitMessageBuilder;

            return message;
        }
    }
}