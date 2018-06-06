namespace Fanex.Bot.Dialogs.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Controllers;
    using Fanex.Bot.Models;
    using Fanex.Bot.Services;
    using Hangfire;
    using Microsoft.Bot.Connector;
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

        public async Task HandleLogMessageAsync(Activity activity, string message)
        {
            if (message.StartsWith("log add"))
            {
                var logCategories = message.Substring(7).Trim();

                if (string.IsNullOrEmpty(logCategories))
                {
                    await SendMissingLogCategoriesMessage(activity);
                    return;
                }

                await AddLogCategoriesAsync(activity, logCategories);
            }
            else if (message.StartsWith("log remove"))
            {
                var logCategories = message.Substring(10).Trim();

                if (string.IsNullOrEmpty(logCategories))
                {
                    await SendMissingLogCategoriesMessage(activity);
                    return;
                }

                await RemoveLogCategoriesAsync(activity, logCategories);
            }
            else if (message.StartsWith("log viewstatus"))
            {
                await GetLogCategoriesAsync(activity);
            }
            else if (message.StartsWith("log start"))
            {
                await StartNotifyingLogAsync(activity);
            }
            else if (message.StartsWith("log stop"))
            {
                await StopNotifyingLogAsync(activity);
            }
            else if (message.StartsWith("log adminstopall"))
            {
                await StopNotifyingLogAllAsync(activity);
            }
            else if (message.StartsWith("log adminstartall"))
            {
                await StartNotifyingLogAllAsync(activity);
            }
            else if (message.StartsWith("log detail"))
            {
                var logId = message.Substring(10).Trim();

                if (string.IsNullOrEmpty(logId))
                {
                    await SendActivity(activity, "I need [LogId].");
                    return;
                }

                await GetLogDetailAsync(activity, logId);
            }
            else
            {
                await SendActivity(activity, MessagesController.GetCommandMessages());
            }
        }

        public async Task StartNotifyingLogAsync(Activity activity)
        {
            var messageInfo = GetMessageInfo(activity);
            messageInfo.IsActive = true;
            await SaveMessageInfoAsync(messageInfo);

            RecurringJob.AddOrUpdate("NotifyLogPeriodically", () => GetAndSendLogAsync(), Cron.Minutely);

            await SendActivity(activity, "Log will be sent to you soon!");
        }

        public async Task StopNotifyingLogAsync(Activity activity)
        {
            var messageInfo = GetMessageInfo(activity);
            messageInfo.IsActive = false;
            await SaveMessageInfoAsync(messageInfo);

            await SendActivity(activity, "Log will not be sent to you more!");
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

        public async Task RemoveLogCategoriesAsync(Activity activity, string logCategories)
        {
            var messageInfo = GetMessageInfo(activity);
            var logCategoryList = logCategories.Split(";");

            foreach (var logCategory in logCategoryList)
            {
                messageInfo.LogCategories = messageInfo.LogCategories.Replace(logCategory, "");
            }

            await SaveMessageInfoAsync(messageInfo);

            await SendActivity(activity, $"You will not receive log with categories contain **[{logCategories}]**");
        }

        public async Task AddLogCategoriesAsync(Activity activity, string logCategories)
        {
            var messageInfo = GetMessageInfo(activity);
            messageInfo.LogCategories += $"{logCategories};";
            await SaveMessageInfoAsync(messageInfo);

            await SendActivity(activity, $"You will receive log with categories contain **[{logCategories}]**");
        }

        public async Task GetLogCategoriesAsync(Activity activity)
        {
            var messageInfo = GetMessageInfo(activity);

            var message = $"Your log status \n\n" +
                $"**Log Categories:** [{messageInfo.LogCategories}]\n\n";

            message += messageInfo.IsActive ? $"**Running**\n\n" : $"**Stopped**\n\n";

            await SendActivity(activity, message);
        }

        public async Task StopNotifyingLogAllAsync(Activity activity)
        {
            if (!CheckAdmin(activity))
            {
                await SendActivity(activity, "Sorry! You are not admin.");
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

        public async Task StartNotifyingLogAllAsync(Activity activity)
        {
            if (!CheckAdmin(activity))
            {
                await SendActivity(activity, "Sorry! You are not admin.");
                return;
            }

            await SendActivity(activity, "Your request is accepted!");

            var messageInfos = _dbContext.MessageInfo;

            foreach (var messageInfo in messageInfos)
            {
                messageInfo.IsActive = true;
            }

            await _dbContext.SaveChangesAsync();
            await SendAdminAsync("All clients is active now!");
        }

        public async Task GetLogDetailAsync(Activity activity, string logId)
        {
            var logDetail = await _logService.GetErrorLogDetail(Convert.ToInt64(logId));

            await SendActivity(activity, logDetail.FullMessage);
        }

        private bool CheckAdmin(Activity activity)
        {
            var currentConversationId = activity.Conversation.Id;
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

        private async Task SendMissingLogCategoriesMessage(Activity activity)
        {
            await SendActivity(activity, "You need to add [LogCategory], otherwise, you will not get any log info");
        }
    }
}