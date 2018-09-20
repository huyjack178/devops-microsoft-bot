namespace Fanex.Bot.Skynex.MessageHandlers.MessageBuilders
{
    using System.Text;
    using Fanex.Bot.Helpers;
    using Fanex.Bot.Models.GitLab;

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

            var message = $"{MessageFormatSignal.BeginBold}GitLab Master Branch Change{MessageFormatSignal.EndBold} (bell){MessageFormatSignal.NewLine}" +
                            $"{MessageFormatSignal.BeginBold}Repository:{MessageFormatSignal.EndBold} {project.WebUrl}{MessageFormatSignal.NewLine}";

            var commitMessageBuilder = new StringBuilder();
            commitMessageBuilder.Append($"{MessageFormatSignal.BeginBold}Commits:{MessageFormatSignal.EndBold}{MessageFormatSignal.NewLine}");

            foreach (var commit in commits)
            {
                var commitUrl = $"{project.WebUrl}/commit/{commit.Id}";

                commitMessageBuilder
                    .Append($"{MessageFormatSignal.BeginBold}[{commit.Id.Substring(0, 8)}]({commitUrl}){MessageFormatSignal.EndBold}")
                    .Append($" {commit.Message} ({commit.Author.Name})")
                    .Append(MessageFormatSignal.NewLine);
            }

            commitMessageBuilder.Append($"{MessageFormatSignal.BreakLine}{MessageFormatSignal.NewLine}");

            message += commitMessageBuilder;

            return message;
        }
    }
}