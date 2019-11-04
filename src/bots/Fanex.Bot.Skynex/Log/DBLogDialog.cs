using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Log.Services;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Hangfire;
using Hangfire.Common;
using Microsoft.Bot.Connector;

namespace Fanex.Bot.Skynex.Log
{
    public interface IDBLogDialog : IDialog
    {
        Task GetAndSendLogAsync();
    }

    public class DBLogDialog : BaseDialog, IDBLogDialog
    {
        private const string NotifyDBLogJobId = "NotifyDbLogPeriodically";
        private const int LoopTimes = 11;
        private readonly ILogService logService;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IDBLogMessageBuilder messageBuilder;

        public DBLogDialog(
            BotDbContext dbContext,
            IConversation conversation,
            ILogService logService,
            IRecurringJobManager recurringJobManager,
            IDBLogMessageBuilder messageBuilder) :
                base(dbContext, conversation)
        {
            this.logService = logService;
            this.recurringJobManager = recurringJobManager;
            this.messageBuilder = messageBuilder;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var command = message.Replace(FunctionType.LogDb.DisplayName, string.Empty).Trim();

            if (command.StartsWith(MessageCommand.START))
            {
                await StartNotifyingDbLogAsync(activity);
                return;
            }

            if (command.StartsWith(MessageCommand.STOP))
            {
                await StopNotifyingDbLogAsync(activity);
            }
        }

        public async Task StartNotifyingDbLogAsync(IMessageActivity activity)
        {
            recurringJobManager.AddOrUpdate(
                NotifyDBLogJobId, Job.FromExpression(() => GetAndSendLogAsync()), Cron.Minutely());

            await Conversation.ReplyAsync(activity, "Database Log has been started!");
        }

        public async Task StopNotifyingDbLogAsync(IMessageActivity activity)
        {
            recurringJobManager.RemoveIfExists(NotifyDBLogJobId);

            await Conversation.ReplyAsync(activity, "Database Log has been stopped!");
        }

        public async Task GetAndSendLogAsync()
        {
            for (int i = 0; i < LoopTimes; i++)
            {
                var dbLogs = await logService.GetDBLogs();

                if (dbLogs == null || !dbLogs.Any())
                {
                    return;
                }

                var successfulSentLogNotificationIds = new List<int>();

                foreach (var log in dbLogs)
                {
                    var message = messageBuilder.BuildMessage(log);
                    var result = await Conversation.SendAsync(log.SkypeGroupId, message);

                    if (result.IsOk)
                    {
                        successfulSentLogNotificationIds.Add(log.NotificationId);
                    }
                }

                if (successfulSentLogNotificationIds.Count > 0)
                {
                    await logService.AckDBLog(successfulSentLogNotificationIds.ToArray());
                }

#pragma warning disable S109 // Magic numbers should not be used
                await Task.Delay(5000);
#pragma warning restore S109 // Magic numbers should not be used
            }
        }
    }
}