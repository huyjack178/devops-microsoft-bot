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
                if (messageParts[1] == "start")
                {
                    await EnableDisableLog(activity, messageParts, true);
                }
                else if (messageParts[1] == "stop")
                {
                    await EnableDisableLog(activity, messageParts, false);
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

        protected async Task EnableDisableLog(IMessageActivity activity, string[] messageParts, bool enabled)
        {
            var enabledMessage = enabled ? "Enabled" : "Disabled";

            if (messageParts.Length > 2)
            {
                var projectName = messageParts[2]?.ToLowerInvariant();
                var sentryInfo = await GetOrCreateSentryInfo(activity, projectName);

                if (sentryInfo != null)
                {
                    sentryInfo.IsActive = enabled;
                    await SaveSentryInfo(sentryInfo);

                    var message = $"Sentry Log has been {enabledMessage} for project " +
                                  $"{MessageFormatSymbol.BOLD_START}{projectName}{MessageFormatSymbol.BOLD_END}!";
                    await Conversation.ReplyAsync(activity, message);
                }
            }
            else
            {
                var sentryInfos = GetAllSentryInfos(activity);

                if (sentryInfos == null)
                {
                    await Conversation.ReplyAsync(activity, "Not found your Sentry notification info");
                    return;
                }

                foreach (var sentryInfo in sentryInfos)
                {
                    sentryInfo.IsActive = enabled;
                    await SaveSentryInfo(sentryInfo);
                }

                await Conversation.ReplyAsync(activity, $"Sentry Log has been {enabledMessage}!");
            }
        }

        public async Task HandlePushEventAsync(PushEvent pushEvent)
        {
            var message = messageBuilder.BuildMessage(pushEvent);
            var project = pushEvent.Project.ToLowerInvariant();

            foreach (var sentryInfo in DbContext.SentryInfo.Where(s => project.StartsWith(s.Project)))
            {
                if (sentryInfo?.IsActive == true && sentryInfo.Level == pushEvent.Level)
                {
                    await Conversation.SendAsync(sentryInfo.ConversationId, message);
                }
            }
        }

        private async Task<SentryInfo> GetOrCreateSentryInfo(IMessageActivity activity, string projectName)
        {
            var sentryInfo = await FindSentryInfo(activity, projectName);

            if (sentryInfo == null)
            {
                sentryInfo = new SentryInfo
                {
                    ConversationId = activity.Conversation.Id,
                    Project = projectName,
                    Level = "error",
                    IsActive = true,
                    CreatedTime = DateTime.UtcNow.AddHours(7)
                };
            }

            return sentryInfo;
        }

        private IEnumerable<SentryInfo> GetAllSentryInfos(IMessageActivity activity)
            => DbContext.SentryInfo.Where(log
                   => log.ConversationId == activity.Conversation.Id);

        private async Task<SentryInfo> FindSentryInfo(IMessageActivity activity, string projectName)
            => await DbContext.SentryInfo.FirstOrDefaultAsync(log
                   => log.ConversationId == activity.Conversation.Id
                      && String.Equals(log.Project, projectName, StringComparison.InvariantCultureIgnoreCase));

        private async Task SaveSentryInfo(SentryInfo sentryInfo)
        {
            var existInfo = DbContext
                    .SentryInfo
                    .AsNoTracking()
                    .Any(e => e.ConversationId == sentryInfo.ConversationId && e.Project == sentryInfo.Project);

            DbContext.Entry(sentryInfo).State = existInfo ? EntityState.Modified : EntityState.Added;

            await DbContext.SaveChangesAsync();
        }
    }
}