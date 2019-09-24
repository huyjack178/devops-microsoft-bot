using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core.Sentry.Models;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;

namespace Fanex.Bot.Skynex.Sentry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public interface ISentryDialog : IDialog
    {
        Task HandlePushEventAsync(PushEvent pushEvent);
    }

    public class SentryDialog : BaseDialog, ISentryDialog
    {
        private readonly int defaultGMT;

        public SentryDialog(
          BotDbContext dbContext,
          IConversation conversation,
          IConfiguration configuration)
          : base(dbContext, conversation)
        {
            defaultGMT = configuration.GetSection("DefaultGMT").Get<int>();
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
                $"{DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(Convert.ToDouble(pushEvent.Event.LogTime))).ToOffset(TimeSpan.FromHours(defaultGMT))}" +
                $"{MessageFormatSignal.NEWLINE}");
            messageBuilder.Append(
                $"{MessageFormatSignal.BOLD_START}Environment:{MessageFormatSignal.BOLD_END} " +
                $"{pushEvent.Event.Environment}{MessageFormatSignal.NEWLINE}");
            messageBuilder.Append(
                $"{MessageFormatSignal.BOLD_START}Message:{MessageFormatSignal.BOLD_END} " +
                $"{pushEvent.Event.Message}{MessageFormatSignal.NEWLINE}");

            if (pushEvent.Event.Request != null)
            {
                var request = pushEvent.Event.Request;
                messageBuilder.Append(
                    $"{MessageFormatSignal.BOLD_START}Request:{MessageFormatSignal.BOLD_END} [{request.Method}] {request.Url}{MessageFormatSignal.NEWLINE}");
            }

            if (pushEvent.Event.Context.Browser != null)
            {
                var browser = pushEvent.Event.Context.Browser;
                messageBuilder.Append(
                    $"{MessageFormatSignal.BOLD_START}Browser:{MessageFormatSignal.BOLD_END} {browser.Name} ({browser.Version}){MessageFormatSignal.NEWLINE}");
            }

            if (pushEvent.Event.Context.Device != null)
            {
                var device = pushEvent.Event.Context.Device;
                messageBuilder.Append(
                    $"{MessageFormatSignal.BOLD_START}Device:{MessageFormatSignal.BOLD_END} {device.Name} ({device.Model}){MessageFormatSignal.NEWLINE}");
            }

            if (pushEvent.Event.Context.Os != null)
            {
                var os = pushEvent.Event.Context.Os;
                messageBuilder.Append(
                    $"{MessageFormatSignal.BOLD_START}OS:{MessageFormatSignal.BOLD_END} {os.Name} {os.Version}{MessageFormatSignal.NEWLINE}");
            }

            messageBuilder.Append(
                $"{MessageFormatSignal.BOLD_START}User:{MessageFormatSignal.BOLD_END} " +
                $"{pushEvent.Event.User.UserName}{MessageFormatSignal.NEWLINE}");

            if (!string.IsNullOrEmpty(pushEvent.Event.User.IpAddress))
            {
                messageBuilder.Append(
                    $"IP Address: {pushEvent.Event.User.IpAddress}{MessageFormatSignal.NEWLINE}");
            }

            if (!string.IsNullOrEmpty(pushEvent.Event.User.Email))
            {
                messageBuilder.Append(
                    $"Email: {pushEvent.Event.User.Email}{MessageFormatSignal.NEWLINE}");
            }

            messageBuilder.Append($"{MessageFormatSignal.NEWLINE}");
            messageBuilder.Append(
              "For more detail, refer to" +
              $" {MessageFormatSignal.BOLD_START}[here]({pushEvent.Url}){MessageFormatSignal.BOLD_END}" +
              $"{MessageFormatSignal.NEWLINE}");
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