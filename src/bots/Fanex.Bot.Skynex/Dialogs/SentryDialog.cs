namespace Fanex.Bot.Skynex.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Sentry;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;

    public interface ISentryDialog : IDialog
    {
        Task HandlePushEventAsync(PushEvent pushEvent);
    }

    public class SentryDialog : BaseDialog, ISentryDialog
    {
        public SentryDialog(
          BotDbContext dbContext,
          IConversation conversation)
          : base(dbContext, conversation)
        {
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var command = message.Replace(MessageCommand.SENTRY_LOG, string.Empty).Trim();

            if (command.StartsWith(MessageCommand.START))
            {
                await EnableLog(activity);
            }
            else if (command.StartsWith(MessageCommand.STOP))
            {
                await DisableLog(activity, message);
            }
            else
            {
                await Conversation.ReplyAsync(activity, GetCommandMessages());
            }
        }

        protected async Task EnableLog(IMessageActivity activity)
        {
            var sentryInfos = GetOrCreateSentryInfos(activity);

            foreach (var sentryInfo in sentryInfos)
            {
                sentryInfo.IsActive = true;
                await SaveSentryInfo(sentryInfo);
            }

            await Conversation.ReplyAsync(activity, "Sentry Log has been enabled!");
        }

        protected async Task DisableLog(IMessageActivity activity, string message)
        {
            var sentryInfos = GetOrCreateSentryInfos(activity);

            foreach (var sentryInfo in sentryInfos)
            {
                sentryInfo.IsActive = false;
                await SaveSentryInfo(sentryInfo);
            }

            await Conversation.ReplyAsync(activity, "Sentry Log has been disabled!");
        }

        public async Task HandlePushEventAsync(PushEvent pushEvent)
        {
            var messageBuilder = new StringBuilder();

            messageBuilder.Append(
                $"{MessageFormatSignal.BOLD_START}Project:{MessageFormatSignal.BOLD_END} " +
                $"{pushEvent.ProjectName}{MessageFormatSignal.NEWLINE}");
            messageBuilder.Append(
                $"{MessageFormatSignal.BOLD_START}Timestamp:{MessageFormatSignal.BOLD_END} " +
                $"{DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(Convert.ToDouble(pushEvent.Event.LogTime)))}{MessageFormatSignal.NEWLINE}");
            messageBuilder.Append(
                $"{MessageFormatSignal.BOLD_START}Message:{MessageFormatSignal.BOLD_END} " +
                $"{pushEvent.Event.Message}{MessageFormatSignal.NEWLINE}");
            messageBuilder.Append(
                $"{MessageFormatSignal.BOLD_START}User:{MessageFormatSignal.BOLD_END} " +
                $"{pushEvent.Event.User.UserName} ({pushEvent.Event.User.Email}){MessageFormatSignal.NEWLINE}");
            messageBuilder.Append(
              $"{MessageFormatSignal.BOLD_START}Url:{MessageFormatSignal.BOLD_END} " +
              $"{pushEvent.Url}{MessageFormatSignal.NEWLINE}");
            messageBuilder.Append($"{MessageFormatSignal.DIVIDER}");

            foreach (var sentryInfo in DbContext.SentryInfo)
            {
                if (sentryInfo.IsActive)
                {
                    await Conversation.SendAsync(sentryInfo.ConversationId, messageBuilder.ToString());
                }
            }
        }

        private IList<SentryInfo> GetOrCreateSentryInfos(IMessageActivity activity)
        {
            var sentryInfos = FindSentryInfos(activity);

            if (sentryInfos == null || sentryInfos.Count == 0)
            {
                var sentryInfo = new SentryInfo
                {
                    ConversationId = activity.Conversation.Id,
                    Project = "all",
                    IsActive = true,
                    CreatedTime = DateTime.UtcNow.AddHours(7)
                };

                sentryInfos = new List<SentryInfo>();
                sentryInfos.Add(sentryInfo);
            }

            return sentryInfos;
        }

        private IList<SentryInfo> FindSentryInfos(IMessageActivity activity)
            => DbContext
                .SentryInfo
                .Where(log => log.ConversationId == activity.Conversation.Id).ToList();

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