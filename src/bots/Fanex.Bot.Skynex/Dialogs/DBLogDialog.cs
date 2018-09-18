namespace Fanex.Bot.Skynex.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Models;
    using Fanex.Bot.Skynex.Services;
    using Fanex.Bot.Skynex.Utilities.Bot;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;

    public interface IDBLogDialog : IDialog
    {
        Task GetAndSendLogAsync();
    }

    public class DBLogDialog : Dialog, IDBLogDialog
    {
        private readonly IUMService umService;
        private readonly ILogService logService;
        private readonly IConfiguration configuration;
        private readonly IRecurringJobManager recurringJobManager;

        public DBLogDialog(
            BotDbContext dbContext,
            IConversation conversation,
            IUMService umService,
            ILogService logService,
            IConfiguration configuration,
            IRecurringJobManager recurringJobManager) :
                base(dbContext, conversation)
        {
            this.umService = umService;
            this.logService = logService;
            this.configuration = configuration;
            this.recurringJobManager = recurringJobManager;
        }

        public override async Task HandleMessageAsync(IMessageActivity activity, string message)
        {
            if (message.StartsWith("dblog start"))
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
            var allowSendLogInUM = Convert.ToBoolean(
                   configuration.GetSection("DBLogInfo")?.GetSection("SendLogInUM")?.Value);

            var umInfo = await umService.GetUMInformation();

            if (!allowSendLogInUM && umInfo.IsUM)
            {
                return;
            }

            var dbLogs = await logService.GetDBLogs();

            foreach (var log in dbLogs)
            {
                await Conversation.SendAsync(log.SkypeGroupId, log.MsgInfo);
            }
        }
    }
}