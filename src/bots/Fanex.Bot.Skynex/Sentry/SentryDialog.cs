using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core.Sentry.Models;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.Bot.Connector;
using Microsoft.EntityFrameworkCore;

namespace Fanex.Bot.Skynex.Sentry
{
    public interface ISentryDialog : IDialog
    {
        Task HandlePushEventAsync(PushEvent pushEvent);
    }

    public class SentryDialog : BaseDialog, ISentryDialog
    {
        private readonly ISentryMessageBuilder messageBuilder;

        public SentryDialog(
          BotDbContext dbContext,
          IConversation conversation,
          ISentryMessageBuilder messageBuilder)
          : base(dbContext, conversation)
        {
            this.messageBuilder = messageBuilder;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var messageParts = message.Split(" ");

            if (messageParts.Length > 1)
            {
                var function = messageParts[1];

                if (function == "start")
                {
                    await EnableLog(activity, messageParts);
                }
                else if (function == "stop")
                {
                    await DisableLog(activity, messageParts);
                }
                else
                {
                    await Conversation.ReplyAsync(activity, GetCommandMessages());
                }
            }
            else
            {
                await Conversation.ReplyAsync(activity, GetCommandMessages());
            }
        }

        private async Task EnableLog(IMessageActivity activity, string[] messageParts)
        {
            if (messageParts.Length <= 2)
            {
                await Conversation.ReplyAsync(activity, "Wrong command!");
                return;
            }

            var (projectName, level) = GetProjectAndLevel(messageParts);

            if (string.IsNullOrEmpty(level))
            {
                var sentryInfos = FindSentryInfos(activity, projectName);

                foreach (var info in sentryInfos)
                {
                    info.IsActive = true;
                    await SaveSentryInfo(info);
                }

                await Conversation.ReplyAsync(activity,
                    "Sentry Log has been ENABLED for " +
                    $"{MessageFormatSymbol.BOLD_START}{projectName}{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}");
                return;
            }
            var sentryInfo = await GetOrCreateSentryInfo(activity, projectName, level);

            if (sentryInfo != null)
            {
                sentryInfo.IsActive = true;
                await SaveSentryInfo(sentryInfo);

                var message = $"Sentry Log has been ENABLED for project " +
                              $"{MessageFormatSymbol.BOLD_START}{projectName}{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}" +
                              $"Log Level: {MessageFormatSymbol.BOLD_START}{level.ToUpperInvariant()}{MessageFormatSymbol.BOLD_END}";
                await Conversation.ReplyAsync(activity, message);
            }
        }

        private async Task DisableLog(IMessageActivity activity, string[] messageParts)
        {
            if (messageParts.Length <= 2)
            {
                await Conversation.ReplyAsync(activity, "Wrong command!");
                return;
            }

            var (projectName, level) = GetProjectAndLevel(messageParts);

            if (string.IsNullOrEmpty(level))
            {
                var sentryInfos = FindSentryInfos(activity, projectName);

                foreach (var info in sentryInfos)
                {
                    info.IsActive = false;
                    await SaveSentryInfo(info);
                }

                await Conversation.ReplyAsync(activity,
                    "Sentry Log has been DISABLED for " +
                    $"{MessageFormatSymbol.BOLD_START}{projectName}{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}");
                return;
            }

            var sentryInfo = await FindSentryInfo(activity, projectName, level);

            if (sentryInfo != null)
            {
                sentryInfo.IsActive = false;
                await SaveSentryInfo(sentryInfo);

                var message = $"Sentry Log has been DISABLED for project " +
                              $"{MessageFormatSymbol.BOLD_START}{projectName}{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}" +
                              $"Log Level: {MessageFormatSymbol.BOLD_START}{level.ToUpperInvariant()}{MessageFormatSymbol.BOLD_END}";
                await Conversation.ReplyAsync(activity, message);
            }
        }

        public async Task HandlePushEventAsync(PushEvent pushEvent)
        {
            var message = messageBuilder.BuildMessage(pushEvent);
            var project = pushEvent.Project.ToLowerInvariant();

            foreach (var sentryInfo in DbContext.SentryInfo.Where(s => project.Equals(s.Project, StringComparison.InvariantCultureIgnoreCase)))
            {
                if (sentryInfo?.IsActive == true && sentryInfo.Level == pushEvent.Level)
                {
                    await Conversation.SendAsync(sentryInfo.ConversationId, message);
                }
            }
        }

        private async Task<SentryInfo> GetOrCreateSentryInfo(IMessageActivity activity, string projectName, string level = "error")
        {
            var sentryInfo = await FindSentryInfo(activity, projectName, level);

            if (sentryInfo == null)
            {
                sentryInfo = new SentryInfo
                {
                    ConversationId = activity.Conversation.Id,
                    Project = projectName,
                    Level = level,
                    IsActive = true,
#pragma warning disable S109 // Magic numbers should not be used
                    CreatedTime = DateTime.UtcNow.AddHours(7)
#pragma warning restore S109 // Magic numbers should not be used
                };
            }

            return sentryInfo;
        }

        private IEnumerable<SentryInfo> GetAllSentryInfos(IMessageActivity activity)
            => DbContext.SentryInfo.Where(log
                   => log.ConversationId == activity.Conversation.Id);

        private IEnumerable<SentryInfo> FindSentryInfos(IMessageActivity activity, string projectName)
            => DbContext.SentryInfo.Where(log =>
                    log.ConversationId == activity.Conversation.Id
                    && string.Equals(log.Project, projectName, StringComparison.InvariantCultureIgnoreCase));

        private async Task<SentryInfo> FindSentryInfo(IMessageActivity activity, string projectName, string level)
            => await DbContext.SentryInfo.FirstOrDefaultAsync(log
                   => log.ConversationId == activity.Conversation.Id
                      && string.Equals(log.Project, projectName, StringComparison.InvariantCultureIgnoreCase)
                      && string.Equals(log.Level, level, StringComparison.InvariantCultureIgnoreCase));

        private async Task SaveSentryInfo(SentryInfo sentryInfo)
        {
            var existInfo = DbContext
                    .SentryInfo
                    .AsNoTracking()
                    .Any(e => e.ConversationId == sentryInfo.ConversationId && e.Project == sentryInfo.Project && e.Level == sentryInfo.Level);

            DbContext.Entry(sentryInfo).State = existInfo ? EntityState.Modified : EntityState.Added;

            await DbContext.SaveChangesAsync();
        }

        private static (string, string) GetProjectAndLevel(string[] messageParts)
        {
            var projectName = messageParts[2]?.ToLowerInvariant();
            var level = string.Empty;
            if (messageParts.Length > 4 && messageParts[3] == "level")
            {
                level = messageParts[4];
            }

            return (projectName, level);
        }
    }
}