using System;
using System.Collections.Generic;
using System.Text;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core.GitLab.Models;
using Fanex.Bot.Core.GitLab.Models.JobEvents;
using Fanex.Bot.Helpers;
using Fanex.Bot.Skynex._Shared.Base;

namespace Fanex.Bot.Skynex.GitLab
{
    public interface IGitLabMessageBuilder : IMessageBuilder
    {
        string BuildJobMessage(JobEvent jobEvent);
    }

    public class GitLabMessageBuilder : IGitLabMessageBuilder
    {
        private const int HexLength = 8;

        private readonly IDictionary<JobStatus, string> StatusIconMapper = new Dictionary<JobStatus, string>
        {
            { JobStatus.Created, ":clock3:" },
            { JobStatus.Running, ":high_brightness:" },
            { JobStatus.Success, ":white_check_mark:" },
            { JobStatus.Failed, ":no_entry:" },
            { JobStatus.Canceled, "" }
        };

        public string BuildMessage(object model)
        {
            var gitlabPushEvent = DataHelper.Parse<PushEvent>(model);
            var project = gitlabPushEvent.Project;
            var commits = gitlabPushEvent.Commits;

            var message = $"{MessageFormatSymbol.BOLD_START}GitLab Master Branch Change{MessageFormatSymbol.BOLD_END} {MessageFormatSymbol.BELL}{MessageFormatSymbol.NEWLINE}" +
                            $"{MessageFormatSymbol.BOLD_START}Repository:{MessageFormatSymbol.BOLD_END} {project.WebUrl}{MessageFormatSymbol.NEWLINE}";

            var commitMessageBuilder = new StringBuilder();
            commitMessageBuilder.Append($"{MessageFormatSymbol.BOLD_START}Commits:{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}");

            foreach (var commit in commits)
            {
                var commitUrl = $"{project.WebUrl}/commit/{commit.Id}";

                commitMessageBuilder
                    .Append($"[{commit.Id.Substring(0, HexLength)}]({commitUrl})")
                    .Append($" {commit.Message} ({commit.Author.Name})")
                    .Append(MessageFormatSymbol.NEWLINE);
            }

            commitMessageBuilder.Append(MessageFormatSymbol.DIVIDER + MessageFormatSymbol.NEWLINE);

            message += commitMessageBuilder;

            return message;
        }

        public string BuildJobMessage(JobEvent jobEvent)
        {
            var messageBuilder = new StringBuilder();
            var statusIcon = StatusIconMapper[jobEvent.Status];

            messageBuilder.Append(
                $"{statusIcon}{MessageFormatSymbol.BOLD_START}{jobEvent.BuildStatus.ToUpperInvariant()}{MessageFormatSymbol.BOLD_END}{statusIcon} " +
                $"CI Job「{jobEvent.BuildName.ToUpperInvariant()}」:rocket: {MessageFormatSymbol.NEWLINE}");
            messageBuilder.Append($"{MessageFormatSymbol.BOLD_START}Project:{MessageFormatSymbol.BOLD_END} {jobEvent.Project.Name}{MessageFormatSymbol.NEWLINE}");
            messageBuilder.Append($"{MessageFormatSymbol.BOLD_START}User Trigger:{MessageFormatSymbol.BOLD_END} {jobEvent.User.Name}{MessageFormatSymbol.NEWLINE}");

            if (jobEvent.Status.IsSuccess || jobEvent.Status.IsFailed)
            {
                var duration = Math.Round(double.Parse(jobEvent.BuildDuration));
                messageBuilder.Append($"{MessageFormatSymbol.BOLD_START}Duration:{MessageFormatSymbol.BOLD_END} {duration} seconds {MessageFormatSymbol.NEWLINE}");
            }

            messageBuilder.Append($"Refer [here]({jobEvent.Project.Url}/-/jobs/{jobEvent.BuildId}) for more detail{MessageFormatSymbol.NEWLINE}");
            messageBuilder.Append(MessageFormatSymbol.DIVIDER + MessageFormatSymbol.NEWLINE);

            return messageBuilder.ToString();
        }
    }
}