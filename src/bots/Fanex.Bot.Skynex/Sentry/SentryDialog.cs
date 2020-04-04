﻿using System;
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
                    await EnableDisableLog(activity, messageParts, true);
                }
                else if (function == "stop")
                {
                    await EnableDisableLog(activity, messageParts, false);
                }
                else if ((function == "enable" || function == "disable") && messageParts.Length > 2)
                {
                    var command = messageParts[2];

                    if (command == "level" && messageParts.Length > 4)
                    {
                        var level = messageParts[3];
                        var project = messageParts[4];
                        await EnableDisableLogLevel(activity, level, project, function == "enable");
                    }
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

#pragma warning disable S109 // Magic numbers should not be used
            if (messageParts.Length > 2)
#pragma warning restore S109 // Magic numbers should not be used
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

        private async Task EnableDisableLogLevel(IMessageActivity activity, string level, string project, bool isEnabled)
        {
            var sentryInfo = await GetOrCreateSentryInfo(activity, project, level);
            var enabledMessage = isEnabled ? "Enabled" : "Disabled";
            sentryInfo.IsActive = isEnabled;

            await SaveSentryInfo(sentryInfo);
            var message = $"Sentry Log {level} has been {enabledMessage} for project " +
                          $"{MessageFormatSymbol.BOLD_START}{project}{MessageFormatSymbol.BOLD_END}!";
            await Conversation.ReplyAsync(activity, message);
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

        private async Task<SentryInfo> FindSentryInfo(IMessageActivity activity, string projectName)
            => await DbContext.SentryInfo.FirstOrDefaultAsync(log
                   => log.ConversationId == activity.Conversation.Id
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
                    .Any(e => e.ConversationId == sentryInfo.ConversationId && e.Project == sentryInfo.Project);

            DbContext.Entry(sentryInfo).State = existInfo ? EntityState.Modified : EntityState.Added;

            await DbContext.SaveChangesAsync();
        }
    }
}