namespace Fanex.Bot.Dialogs.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Services;
    using Fanex.Bot.Utilitites.Bot;
    using Hangfire;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class LogDialog : Dialog, ILogDialog
    {
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;
        private readonly BotDbContext _dbContext;

        public LogDialog(
            ILogService logService,
            IConfiguration configuration,
            BotDbContext dbContext,
            IConversation conversation)
                : base(configuration, dbContext, conversation)
        {
            _configuration = configuration;
            _logService = logService;
            _dbContext = dbContext;
        }

        public async Task HandleMessageAsync(IMessageActivity activity, string message)
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
                await GetLogInfoAsync(activity);
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
                await EnableNotifyingLogAllAsync(activity, false);
            }
            else if (message.StartsWith("log adminstartall"))
            {
                await EnableNotifyingLogAllAsync(activity, true);
            }
            else if (message.StartsWith("log detail"))
            {
                var logId = message.Substring(10).Trim();

                if (string.IsNullOrEmpty(logId))
                {
                    await Conversation.SendAsync(activity, "I need [LogId].");
                    return;
                }

                await GetLogDetailAsync(activity, logId);
            }
            else
            {
                await Conversation.SendAsync(activity, GetCommandMessages());
            }
        }

        public async Task StartNotifyingLogAsync(IMessageActivity activity)
        {
            var logInfo = GetLogInfo(activity);
            logInfo.IsActive = true;
            await SaveLogInfoAsync(logInfo);
            await RegisterMessageInfo(activity);

            RecurringJob.AddOrUpdate("NotifyLogPeriodically", () => GetAndSendLogAsync(), Cron.Minutely);

            await Conversation.SendAsync(activity, "Log will be sent to you soon!");
        }

        public async Task GetAndSendLogAsync()
        {
            var logInfos = _dbContext.LogInfo.ToList();
            var errorLogs = await _logService.GetErrorLogs();

            foreach (var logInfo in logInfos)
            {
                if (logInfo.IsActive)
                {
                    await SendLogAsync(errorLogs, logInfo);
                }
            }
        }

        public async Task StopNotifyingLogAsync(IMessageActivity activity)
        {
            var logInfo = GetLogInfo(activity);
            logInfo.IsActive = false;
            await SaveLogInfoAsync(logInfo);

            await Conversation.SendAsync(activity, "Log will not be sent to you more!");
        }

        public async Task RemoveLogCategoriesAsync(IMessageActivity activity, string logCategories)
        {
            var logCategoryList = logCategories.Split(";");
            var logInfo = GetLogInfo(activity);

            foreach (var logCategory in logCategoryList)
            {
                logInfo.LogCategories = logInfo.LogCategories.Replace(logCategory, "");
            }

            await Conversation.SendAsync(activity, $"You will not receive log with categories contain **[{logCategories}]**");
        }

        public async Task AddLogCategoriesAsync(IMessageActivity activity, string logCategories)
        {
            var isDisableAddCategories = Convert.ToBoolean(_configuration.GetSection("LogInfo")?.GetSection("DisableAddCategories")?.Value);

            if (!CheckAdmin(activity) && isDisableAddCategories)
            {
                await Conversation.SendAsync(activity, $"Add log categories is disabled, please contact NexOps.");
                return;
            }

            var logInfo = GetLogInfo(activity);
            logInfo.LogCategories += $"{logCategories};";
            await SaveLogInfoAsync(logInfo);

            await Conversation.SendAsync(activity, $"You will receive log with categories contain **[{logCategories}]**");
        }

        public async Task GetLogInfoAsync(IMessageActivity activity)
        {
            var logInfo = GetLogInfo(activity);

            var message = $"Your log status \n\n" +
                $"**Log Categories:** [{logInfo.LogCategories}]\n\n";

            message += logInfo.IsActive ? $"**Running**\n\n" : $"**Stopped**\n\n";

            await Conversation.SendAsync(activity, message);
        }

        public async Task GetLogDetailAsync(IMessageActivity activity, string logId)
        {
            var logDetail = await _logService.GetErrorLogDetail(Convert.ToInt64(logId));

            await Conversation.SendAsync(activity, logDetail.FullMessage);
        }

        public async Task EnableNotifyingLogAllAsync(IMessageActivity activity, bool isEnable)
        {
            if (!CheckAdmin(activity))
            {
                await Conversation.SendAsync(activity, "Sorry! You are not admin.");
                return;
            }

            await Conversation.SendAsync(activity, "Your request is accepted!");

            var logInfos = _dbContext.LogInfo;

            foreach (var logInfo in logInfos)
            {
                logInfo.IsActive = isEnable;
            }

            await _dbContext.SaveChangesAsync();
            var status = isEnable ? "active" : "inactive";
            await Conversation.SendAdminAsync($"All clients is {status} now!");
        }

        #region Private Methods

        private LogInfo GetLogInfo(IMessageActivity activity)
        {
            var logInfo = _dbContext.LogInfo.FirstOrDefault(log => log.ConversationId == activity.Conversation.Id);

            if (logInfo == null)
            {
                logInfo = new LogInfo
                {
                    ConversationId = activity.Conversation.Id,
                    LogCategories = string.Empty,
                    IsActive = true,
                    CreatedTime = DateTime.UtcNow.AddHours(7)
                };
            }

            return logInfo;
        }

        private async Task SaveLogInfoAsync(LogInfo logInfo)
        {
            _dbContext.Entry(logInfo).State =
                _dbContext.LogInfo.Any(e => e.ConversationId == logInfo.ConversationId) ?
                    EntityState.Modified : EntityState.Added;

            await _dbContext.SaveChangesAsync();
        }

        private bool CheckAdmin(IMessageActivity activity)
        {
            var currentConversationId = activity.Conversation.Id;
            var isAdmin = _dbContext.MessageInfo.Any(
                messageInfo => messageInfo.IsAdmin &&
                messageInfo.ConversationId == currentConversationId);

            var currentFromId = activity.From.Id;
            var isFromAdmin = _dbContext.MessageInfo.Any(
                messageInfo => (messageInfo.IsAdmin &&
                messageInfo.ConversationId == currentFromId &&
                messageInfo.ToId == currentFromId));

            return isAdmin || isFromAdmin;
        }

        private async Task SendLogAsync(IEnumerable<Log> errorLogs, LogInfo logInfo)
        {
            var messageInfo = _dbContext.MessageInfo.FirstOrDefault(m => m.ConversationId == logInfo.ConversationId);
            var filterCategories = logInfo
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
                    await Conversation.SendAsync(messageInfo);
                }
            }
        }

        private async Task SendMissingLogCategoriesMessage(IMessageActivity activity)
            => await Conversation.SendAsync(activity, "You need to add [LogCategory], otherwise, you will not get any log info");

        #endregion Private Methods
    }
}