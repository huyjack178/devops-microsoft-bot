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
            BotDbContext dbContext)
                : base(configuration, dbContext)
        {
            _logService = logService;
            _dbContext = dbContext;
        }

        public async Task HandleLogMessageAsync(ITurnContext context, string message)
        {
            if (message.StartsWith("log add"))
            {
                var logCategories = message.Substring(7).Trim();

                if (string.IsNullOrEmpty(logCategories))
                {
                    await SendMissingLogCategoriesMessage(context);
                    return;
                }

                await AddLogCategoriesAsync(context, logCategories);
            }
            else if (message.StartsWith("log remove"))
            {
                var logCategories = message.Substring(10).Trim();

                if (string.IsNullOrEmpty(logCategories))
                {
                    await SendMissingLogCategoriesMessage(context);
                    return;
                }

                await RemoveLogCategoriesAsync(context, logCategories);
            }
            else if (message.StartsWith("log viewstatus"))
            {
                await GetLogCategoriesAsync(context);
            }
            else if (message.StartsWith("log start"))
            {
                await StartNotifyingLogAsync(context);
            }
            else if (message.StartsWith("log stop"))
            {
                await StopNotifyingLogAsync(context);
            }
            else if (message.StartsWith("log adminstopall"))
            {
                await StopNotifyingLogAllAsync(context);
            }
            else if (message.StartsWith("log adminstartall"))
            {
                await StartNotifyingLogAllAsync(context);
            }
            else if (message.StartsWith("log detail"))
            {
                var logId = message.Substring(10).Trim();

                if (string.IsNullOrEmpty(logId))
                {
                    await context.SendActivity("I need [LogId].");
                    return;
                }

                await GetLogDetailAsync(context, logId);
            }
            else
            {
                await context.SendActivity(Bot.GetCommandMessages());
            }
        }

        public async Task StartNotifyingLogAsync(ITurnContext context)
        {
            var messageInfo = GetMessageInfo(context);
            messageInfo.IsActive = true;
            await SaveMessageInfoAsync(messageInfo);

            RecurringJob.AddOrUpdate("NotifyLogPeriodically", () => GetAndSendLogAsync(), Cron.Minutely);

            await context.SendActivity("Log will be sent to you soon!");
        }

        public async Task StopNotifyingLogAsync(ITurnContext context)
        {
            var messageInfo = GetMessageInfo(context);
            messageInfo.IsActive = false;
            await SaveMessageInfoAsync(messageInfo);

            await context.SendActivity("Log will not be sent to you more!");
        }

        public async Task GetAndSendLogAsync()
        {
            var messageInfos = _dbContext.MessageInfo.ToList();

            var errorLogs = await _logService.GetErrorLogs();

            foreach (var messageInfo in messageInfos)
            {
                if (messageInfo.IsActive)
                {
                    await SendLogAsync(errorLogs, messageInfo);
                }
            }
        }

        public async Task RemoveLogCategoriesAsync(ITurnContext context, string logCategories)
        {
            var messageInfo = GetMessageInfo(context);
            var logCategoryList = logCategories.Split(";");

            foreach (var logCategory in logCategoryList)
            {
                messageInfo.LogCategories = messageInfo.LogCategories.Replace(logCategory, "");
            }

            await SaveMessageInfoAsync(messageInfo);

            await context.SendActivity($"You will not receive log with categories contain **[{logCategories}]**");
        }

        public async Task AddLogCategoriesAsync(ITurnContext context, string logCategories)
        {
            var messageInfo = GetMessageInfo(context);
            messageInfo.LogCategories += $"{logCategories};";
            await SaveMessageInfoAsync(messageInfo);

            await context.SendActivity($"You will receive log with categories contain **[{logCategories}]**");
        }

        public async Task GetLogCategoriesAsync(ITurnContext context)
        {
            var messageInfo = GetMessageInfo(context);

            var message = $"Your log status \n\n" +
                $"**Log Categories:** [{messageInfo.LogCategories}]\n\n";

            message += messageInfo.IsActive ? $"**Running**\n\n" : $"**Stopped**\n\n";

            await context.SendActivity(message);
        }

        public async Task StopNotifyingLogAllAsync(ITurnContext context)
        {
            if (!CheckAdmin(context))
            {
                await context.SendActivity("Sorry! You are not admin.");
                return;
            }

            var messageInfos = _dbContext.MessageInfo;

            foreach (var messageInfo in messageInfos)
            {
                messageInfo.IsActive = false;
            }

            await _dbContext.SaveChangesAsync();
            await SendAdminAsync("All clients is inactive now!");
        }

        public async Task StartNotifyingLogAllAsync(ITurnContext context)
        {
            if (!CheckAdmin(context))
            {
                await context.SendActivity("Sorry! You are not admin.");
                return;
            }

            await context.SendActivity("Your request is accepted!");

            var messageInfos = _dbContext.MessageInfo;

            foreach (var messageInfo in messageInfos)
            {
                messageInfo.IsActive = true;
            }

            await _dbContext.SaveChangesAsync();
            await SendAdminAsync("All clients is active now!");
        }

        public async Task GetLogDetailAsync(ITurnContext context, string logId)
        {
            var logDetail = await _logService.GetErrorLogDetail(Convert.ToInt64(logId));

            await context.SendActivity(logDetail.FullMessage);
        }

        private bool CheckAdmin(ITurnContext context)
        {
            var currentConversationId = context.Activity.Conversation.Id;
            var isAdmin = _dbContext.MessageInfo.Any(
                messageInfo => messageInfo.IsAdmin &&
                messageInfo.ConversationId == currentConversationId);

            return isAdmin;
        }

        private async Task SendLogAsync(IEnumerable<Log> errorLogs, MessageInfo messageInfo)
        {
            var filterCategories = messageInfo
                                    .LogCategories?.Split(";")
                                    .Where(category => !string.IsNullOrEmpty(category));
            var groupErrorLogs = errorLogs.GroupBy(log => new { log.Category.CategoryName, log.Machine.MachineIP });

            foreach (var groupErrorLog in groupErrorLogs)
            {
                var errorLog = groupErrorLog.First();
                var logCategory = errorLog.Category.CategoryName.ToLowerInvariant();
                var hasLogCategory = filterCategories.Any(filterCategory => logCategory.Contains(filterCategory.ToLowerInvariant()));

                if (hasLogCategory)
                {
                    messageInfo.Text = errorLog.Message;
                    await SendAsync(messageInfo);
                }
            }
        }

        private static async Task SendMissingLogCategoriesMessage(ITurnContext context)
        {
            await context.SendActivity("You need to add [LogCategory], otherwise, you will not get any log info");
        }
    }
}