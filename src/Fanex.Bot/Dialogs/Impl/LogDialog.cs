namespace Fanex.Bot.Dialogs.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Services;
    using Hangfire;
    using Microsoft.Bot.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    public class LogDialog : BaseDialog, ILogDialog
    {
        private readonly ILogService _logService;
        private readonly BotDbContext _dbContext;

        public LogDialog(
            ILogService logService,
            IConfiguration configuration,
            BotDbContext dbContext,
            IOptions<MessageInfo> adminMessageInfo)
                : base(configuration, dbContext, adminMessageInfo)
        {
            _logService = logService;
            _dbContext = dbContext;
        }

        public async Task NotifyLogAsync(ITurnContext context)
        {
            RecurringJob.AddOrUpdate("NotifyLogPeriodically", () => GetAndSendLogAsync(), Cron.Minutely);

            await context.SendActivity("Log will be sent to you soon!");
        }

        public async Task RegisterLogCategoryAsync(ITurnContext context, string logCategory)
        {
            var message = context.Activity;
            var messageInfo = _dbContext.MessageInfo.FirstOrDefault(e => e.ConversationId == message.Conversation.Id);

            if (messageInfo == null)
            {
                messageInfo = GenerateMessageInfo(context);
            }

            messageInfo.LogCategory += $"{logCategory};";

            await SaveMessageInfo(messageInfo);
        }

        public async Task GetAndSendLogAsync()
        {
            var messageInfos = _dbContext.MessageInfo.ToList();

            var errorLogs = await _logService.GetErrorLogs();

            foreach (var messageInfo in messageInfos)
            {
                await SendLogAsync(errorLogs, messageInfo);
            }
        }

        private async Task SendLogAsync(IEnumerable<Log> errorLogs, MessageInfo messageInfo)
        {
            var filterCategories = messageInfo.LogCategory?.Split(";");
            //var groupErrorLogs = errorLogs.GroupBy(log => new { log.Category.CategoryName, log.Machine.MachineIP });

            foreach (var errorLog in errorLogs)
            {
                var logCategory = errorLog.Category.CategoryName.ToLowerInvariant();
                var hasLogCategory = filterCategories.Any(filterCategory => logCategory.Contains(filterCategory.ToLowerInvariant()));

                if (hasLogCategory)
                {
                    messageInfo.Text = errorLog.Message;
                    await SendAsync(messageInfo);
                }
            }
        }
    }
}