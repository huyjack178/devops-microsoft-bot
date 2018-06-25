namespace Fanex.Bot.Dialogs.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Services;
    using Fanex.Bot.Utilitites.Bot;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Fanex.Bot.Utilities.Common;
    using Microsoft.Extensions.Caching.Memory;

    public class LogDialog : Dialog, ILogDialog
    {
        private readonly IConfiguration _configuration;
        private readonly ILogService _logService;
        private readonly BotDbContext _dbContext;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IMemoryCache _cache;

        public LogDialog(
            IConfiguration configuration,
            ILogService logService,
            BotDbContext dbContext,
            IConversation conversation,
            IRecurringJobManager recurringJobManager,
            IBackgroundJobClient backgroundJobClient,
            IMemoryCache cache)
                : base(dbContext, conversation)
        {
            _configuration = configuration;
            _logService = logService;
            _dbContext = dbContext;
            _recurringJobManager = recurringJobManager;
            _backgroundJobClient = backgroundJobClient;
            _cache = cache;
        }

        public async Task HandleMessageAsync(IMessageActivity activity, string messageCmd)
        {
            if (messageCmd.StartsWith("log add"))
            {
                await AddLogCategoriesAsync(activity, messageCmd);
            }
            else if (messageCmd.StartsWith("log remove"))
            {
                await RemoveLogCategoriesAsync(activity, messageCmd);
            }
            else if (messageCmd.StartsWith("log status"))
            {
                await GetLogInfoAsync(activity);
            }
            else if (messageCmd.StartsWith("log start"))
            {
                await StartNotifyingLogAsync(activity);
            }
            else if (messageCmd.StartsWith("log stop"))
            {
                await StopNotifyingLogAsync(activity, messageCmd);
            }
            else if (messageCmd.StartsWith("log detail"))
            {
                await GetLogDetailAsync(activity, messageCmd);
            }
            else
            {
                await Conversation.SendAsync(activity, GetCommandMessages());
            }
        }

        public async Task AddLogCategoriesAsync(IMessageActivity activity, string messageCmd)
        {
            var logCategories = messageCmd.Substring(7).Trim();

            if (string.IsNullOrEmpty(logCategories))
            {
                await SendMissingLogCategoriesMessage(activity);
                return;
            }

            var isDisableAddCategories = Convert.ToBoolean(_configuration.GetSection("LogInfo")?.GetSection("DisableAddCategories")?.Value);

            if (!CheckAdmin(activity) && isDisableAddCategories)
            {
                await Conversation.SendAsync(activity, $"Add log categories is disabled, please contact NexOps.");
                return;
            }

            var logInfo = await FindOrCreateLogInfoAsync(activity);
            logInfo.LogCategories += $"{logCategories};";
            await SaveLogInfoAsync(logInfo);

            await Conversation.SendAsync(activity, $"You will receive log with categories contain **[{logCategories}]**");
        }

        public async Task RemoveLogCategoriesAsync(IMessageActivity activity, string messageCmd)
        {
            var logCategories = messageCmd.Substring(10).Trim();

            if (string.IsNullOrEmpty(logCategories))
            {
                await SendMissingLogCategoriesMessage(activity);
                return;
            }

            var logCategoryList = logCategories.Split(';');
            var logInfo = await FindLogInfoAsync(activity);

            if (logInfo == null)
            {
                await Conversation.SendAsync(activity, $"You don't have any log categories data");
                return;
            }

            foreach (var logCategory in logCategoryList)
            {
                logInfo.LogCategories = logInfo.LogCategories.Replace($"{logCategory}", "");
            }

            await SaveLogInfoAsync(logInfo);
            await Conversation.SendAsync(activity, $"You will not receive log with categories contain **[{logCategories}]**");
        }

        public async Task StartNotifyingLogAsync(IMessageActivity activity)
        {
            var logInfo = await FindOrCreateLogInfoAsync(activity);
            logInfo.IsActive = true;
            await SaveLogInfoAsync(logInfo);
            await RegisterMessageInfo(activity);

            RemoveRestartLogJob(activity);

            _recurringJobManager.AddOrUpdate(
                "NotifyLogPeriodically", Job.FromExpression(() => GetAndSendLogAsync()), Cron.Minutely());

            await Conversation.SendAsync(activity, "Log has been started!");
        }

        public async Task StopNotifyingLogAsync(IMessageActivity activity, string messageCmd)
        {
            var logInfo = await FindOrCreateLogInfoAsync(activity);
            logInfo.IsActive = false;
            await SaveLogInfoAsync(logInfo);

            TimeSpan logStopDelayTime = GenerateLogStopTime(messageCmd);

            ScheduleRestartLogJob(activity, logStopDelayTime);

            await Conversation.SendAsync(activity, $"Log has been stopped for {logStopDelayTime.ToReadableString()}");
        }

        public async Task RestartNotifyingLog(string conversationId)
        {
            var messageInfo = await _dbContext.MessageInfo
                    .FirstOrDefaultAsync(info => info.ConversationId == conversationId);
            var logInfo = await _dbContext.LogInfo
                    .FirstOrDefaultAsync(info => info.ConversationId == conversationId);

            if (messageInfo != null && logInfo != null)
            {
                logInfo.IsActive = true;
                await SaveLogInfoAsync(logInfo);

                messageInfo.Text = "Log has been restarted!";
                await Conversation.SendAsync(messageInfo);
            }
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

        public async Task GetLogInfoAsync(IMessageActivity activity)
        {
            var logInfo = await FindOrCreateLogInfoAsync(activity);

            var message = $"Your log status \n\n" +
                $"**Log Categories:** [{logInfo.LogCategories}]\n\n";

            message += logInfo.IsActive ? $"**Running**\n\n" : $"**Stopped**\n\n";

            await Conversation.SendAsync(activity, message);
        }

        public async Task GetLogDetailAsync(IMessageActivity activity, string messageCmd)
        {
            var logId = messageCmd.Substring(10).Trim();

            if (string.IsNullOrEmpty(logId))
            {
                await Conversation.SendAsync(activity, "I need [LogId].");
                return;
            }

            var logDetail = await _logService.GetErrorLogDetail(Convert.ToInt64(logId));

            await Conversation.SendAsync(activity, logDetail.FullMessage);
        }

        #region Private Methods

        private async Task<LogInfo> FindOrCreateLogInfoAsync(IMessageActivity activity)
        {
            var logInfo = await FindLogInfoAsync(activity);

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

        private async Task<LogInfo> FindLogInfoAsync(IMessageActivity activity)
        => await _dbContext
            .LogInfo
            .FirstOrDefaultAsync(log => log.ConversationId == activity.Conversation.Id);

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
            var messageInfo = await _dbContext.MessageInfo.FirstOrDefaultAsync(
                m => m.ConversationId == logInfo.ConversationId);

            if (messageInfo == null)
            {
                return;
            }

            var filterCategories = logInfo
                                    .LogCategories?.Split(';')
                                    .Where(category => !string.IsNullOrEmpty(category));

            var groupErrorLogs = errorLogs.GroupBy(log => new { log.Category.CategoryName, log.Machine.MachineIP });

            foreach (var groupErrorLog in groupErrorLogs)
            {
                var errorLog = groupErrorLog.First();
                var logCategory = errorLog.Category.CategoryName.ToLowerInvariant();
                var hasLogCategory = filterCategories.Any(filterCategory => logCategory.Contains(filterCategory.ToLowerInvariant()));
                var hasIgnoreMessage = await _dbContext.LogIgnoreMessage.AnyAsync(
                        message => logCategory.Contains(message.Category.ToLowerInvariant()) &&
                        errorLog.Message.ToLowerInvariant().Contains(message.IgnoreMessage));

                if (hasLogCategory && !hasIgnoreMessage)
                {
                    messageInfo.Text = errorLog.Message;
                    await Conversation.SendAsync(messageInfo);
                }
            }
        }

        private async Task SendMissingLogCategoriesMessage(IMessageActivity activity)
            => await Conversation.SendAsync(activity, "You need to add [LogCategory], otherwise, you will not get any log info");

        private static TimeSpan GenerateLogStopTime(string messageCmd)
        {
            var timeSpanText = messageCmd.Replace("log stop", string.Empty).Trim();

            TimeSpan logStopDelayTime;

            if (string.IsNullOrEmpty(timeSpanText))
            {
                logStopDelayTime = TimeSpan.FromMinutes(10);
            }
            else
            {
                var timeSpanNumber = Regex.Match(timeSpanText, @"\d+").Value;
                var timeSpanFormat = timeSpanText.Replace(timeSpanNumber, string.Empty);
                TimeSpan.TryParseExact(timeSpanNumber, $"%{timeSpanFormat}", null, out logStopDelayTime);
            }

            return logStopDelayTime;
        }

        private void ScheduleRestartLogJob(IMessageActivity activity, TimeSpan logStopDelayTime)
        {
            RemoveRestartLogJob(activity);

            var jobId = _backgroundJobClient.Schedule(() => RestartNotifyingLog(activity.Conversation.Id), logStopDelayTime);
            _cache.Set(
                activity.Conversation.Id,
                jobId,
                new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });
        }

        private void RemoveRestartLogJob(IMessageActivity activity)
        {
            var restartLogJobId = _cache.Get<string>(activity.Conversation.Id);

            if (!string.IsNullOrEmpty(restartLogJobId))
            {
                _backgroundJobClient.Delete(restartLogJobId);
            }
        }

        #endregion Private Methods
    }
}