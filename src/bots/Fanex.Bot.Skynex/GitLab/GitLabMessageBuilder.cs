using System.Text;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core.GitLab.Models;
using Fanex.Bot.Helpers;
using Fanex.Bot.Skynex._Shared.Base;

namespace Fanex.Bot.Skynex.GitLab
{
    public interface IGitLabMessageBuilder : IMessageBuilder
    {
    }

    public class GitLabMessageBuilder : IGitLabMessageBuilder
    {
        private const int HexLength = 8;

        public string BuildMessage(object model)
        {
            var gitlabPushEvent = DataHelper.Parse<PushEvent>(model);
            var project = gitlabPushEvent.Project;
            var commits = gitlabPushEvent.Commits;

            var message = $"{MessageFormatSymbol.BOLD_START}GitLab Master Branch Change{MessageFormatSymbol.BOLD_END} (bell){MessageFormatSymbol.NEWLINE}" +
                            $"{MessageFormatSymbol.BOLD_START}Repository:{MessageFormatSymbol.BOLD_END} {project.WebUrl}{MessageFormatSymbol.NEWLINE}";

            var commitMessageBuilder = new StringBuilder();
            commitMessageBuilder.Append($"{MessageFormatSymbol.BOLD_START}Commits:{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}");

            foreach (var commit in commits)
            {
                var commitUrl = $"{project.WebUrl}/commit/{commit.Id}";

                commitMessageBuilder
                    .Append($"{MessageFormatSymbol.BOLD_START}[{commit.Id.Substring(0, HexLength)}]({commitUrl}){MessageFormatSymbol.BOLD_END}")
                    .Append($" {commit.Message} ({commit.Author.Name})")
                    .Append(MessageFormatSymbol.NEWLINE);
            }

            commitMessageBuilder.Append(MessageFormatSymbol.DIVIDER + MessageFormatSymbol.NEWLINE);

            message += commitMessageBuilder;

            return message;
        }
    }
}