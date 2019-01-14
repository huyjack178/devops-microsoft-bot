namespace Fanex.Bot.Skynex.Dialogs
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Services;
    using Fanex.Bot.Skynex.MessageHandlers.MessageBuilders;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Bot.Connector;

    public interface IDBLogDialog : IDialog
    {
        Task GetAndSendLogAsync();
    }

    public class DBLogDialog : BaseDialog, IDBLogDialog
    {
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
            var command = message.Replace(MessageCommand.DBLOG, string.Empty).Trim();

            if (command.StartsWith(MessageCommand.Start))
            {
                await StartNotifyingDbLogAsync(activity);
            }
        }

        public async Task StartNotifyingDbLogAsync(IMessageActivity activity)
        {
            recurringJobManager.AddOrUpdate(
                "NotifyDbLogPeriodically", Job.FromExpression(() => GetAndSendLogAsync()), Cron.Minutely());

            await Conversation.ReplyAsync(activity, "DBLog has been started!");
        }

        public async Task GetAndSendLogAsync()
        {
            for (int i = 0; i < 12; i++)
            {
                var dbLogs = await logService.GetDBLogs();

                if (dbLogs == null)
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

                Thread.Sleep(5000);
            }
        }
    }
}