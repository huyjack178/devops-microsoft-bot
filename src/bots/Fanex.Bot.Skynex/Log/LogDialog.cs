using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fanex.Bot.Common.Extensions;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core.Log.Models;
using Fanex.Bot.Core.Log.Services;
using Fanex.Bot.Core.UM.Services;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Hangfire;
using Hangfire.Common;
using Microsoft.Bot.Connector;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Fanex.Bot.Skynex.Log
{
    public interface ILogDialog : IDialog
    {
        Task GetAndSendLogAsync();

        Task RestartNotifyingLog(string conversationId);
    }

    public class LogDialog : BaseDialog, ILogDialog
    {
        private readonly IConfiguration configuration;
        private readonly ILogService logService;
        private readonly IUnderMaintenanceService umService;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IMemoryCache cache;
        private readonly IWebLogMessageBuilder webLogMessageBuilder;

#pragma warning disable S107 // Methods should not have too many parameters

        public LogDialog(
            IConfiguration configuration,
            ILogService logService,
            IUnderMaintenanceService umService,
            BotDbContext dbContext,
            IConversation conversation,
            IRecurringJobManager recurringJobManager,
            IBackgroundJobClient backgroundJobClient,
            IMemoryCache cache,
            IWebLogMessageBuilder webLogMessageBuilder)
#pragma warning restore S107 // Methods should not have too many parameters
                : base(dbContext, conversation)
        {
            this.configuration = configuration;
            this.logService = logService;
            this.umService = umService;
            this.recurringJobManager = recurringJobManager;
            this.backgroundJobClient = backgroundJobClient;
            this.cache = cache;
            this.webLogMessageBuilder = webLogMessageBuilder;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            if (message.StartsWith("log add"))
            {
                await AddLogCategoriesAsync(activity, message);
            }
            else if (message.StartsWith("log remove"))
            {
                await RemoveLogCategoriesAsync(activity, message);
            }
            else if (message.StartsWith("log status"))
            {
                await GetLogInfoAsync(activity);
            }
            else if (message.StartsWith("log start"))
            {
                await StartNotifyingLogAsync(activity);
            }
            else if (message.StartsWith("log stop"))
            {
                await StopNotifyingLogAsync(activity, message);
            }
            else
            {
                await Conversation.ReplyAsync(activity, GetCommandMessages());
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

            var isDisableAddCategories = Convert.ToBoolean(
                configuration.GetSection("LogInfo")?.GetSection("DisableAddCategories")?.Value);

            if (!CheckAdmin(activity) && isDisableAddCategories)
            {
                await Conversation.ReplyAsync(activity, $"Add log categories is disabled, please contact NexOps.");
                return;
            }

            var logInfo = await GetOrCreateLogInfoAsync(activity);
            logInfo.LogCategories += $"{logCategories};";
            await SaveLogInfoAsync(logInfo);

            await Conversation.ReplyAsync(activity,
                $"You will receive log with categories contain {MessageFormatSignal.BOLD_START}[{logCategories}]{MessageFormatSignal.BOLD_END}");
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
                await Conversation.ReplyAsync(activity, $"You don't have any log categories data");
                return;
            }

            foreach (var logCategory in logCategoryList)
            {
                logInfo.LogCategories = logInfo.LogCategories.Replace(logCategory, "");
            }

            await SaveLogInfoAsync(logInfo);
            await Conversation.ReplyAsync(activity,
                $"You will not receive log with categories contain {MessageFormatSignal.BOLD_START}[{logCategories}]{MessageFormatSignal.BOLD_END}");
        }

        public async Task StartNotifyingLogAsync(IMessageActivity activity)
        {
            var logInfo = await GetOrCreateLogInfoAsync(activity);
            logInfo.IsActive = true;
            await SaveLogInfoAsync(logInfo);

            RemoveRestartLogJob(activity);

            recurringJobManager.AddOrUpdate(
                "NotifyLogPeriodically", Job.FromExpression(() => GetAndSendLogAsync()), Cron.Minutely());

            await Conversation.ReplyAsync(activity, "Log has been started!");
        }

        public async Task StopNotifyingLogAsync(IMessageActivity activity, string messageCmd)
        {
            var logInfo = await GetOrCreateLogInfoAsync(activity);
            logInfo.IsActive = false;
            await SaveLogInfoAsync(logInfo);

            TimeSpan logStopDelayTime = GenerateLogStopTime(messageCmd);

            ScheduleRestartLogJob(activity, logStopDelayTime);

            await Conversation.ReplyAsync(activity, $"Log has been stopped for {logStopDelayTime.ToReadableString()}");
        }

        public async Task RestartNotifyingLog(string conversationId)
        {
            var messageInfo = await DbContext.MessageInfo
                    .FirstOrDefaultAsync(info => info.ConversationId == conversationId);
            var logInfo = await DbContext.LogInfo
                    .FirstOrDefaultAsync(info => info.ConversationId == conversationId);

            if (messageInfo != null && logInfo != null)
            {
                logInfo.IsActive = true;
                await SaveLogInfoAsync(logInfo);

                await Conversation.SendAsync(conversationId, "Log has been restarted!");
            }
        }

        public async Task GetAndSendLogAsync()
        {
            var allowSendLogInUM = Convert.ToBoolean(
                    configuration.GetSection("LogInfo")?.GetSection("SendLogInUM")?.Value);
            var isProduction = Convert.ToBoolean(
                    configuration.GetSection("LogInfo")?.GetSection("IsProduction")?.Value ?? "true");
            var actualUnderMaintenanceInformation = await umService.GetActualInfo();

            if (!allowSendLogInUM && actualUnderMaintenanceInformation.Any(info => info.Value.IsUnderMaintenanceTime))
            {
                return;
            }

            var logInfos = DbContext.LogInfo.ToList();

            var errorLogs = await logService.GetErrorLogs(isProduction: isProduction);

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
            var logInfo = await GetOrCreateLogInfoAsync(activity);

            var message = $"Your log status {MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}Log Categories:{MessageFormatSignal.BOLD_END} [{logInfo.LogCategories}]{MessageFormatSignal.NEWLINE}";

            message += logInfo.IsActive ?
                $"{MessageFormatSignal.BOLD_START}Running{MessageFormatSignal.BOLD_END}{MessageFormatSignal.NEWLINE}" :
                $"{MessageFormatSignal.BOLD_START}Stopped{MessageFormatSignal.BOLD_END}{MessageFormatSignal.NEWLINE}";

            await Conversation.ReplyAsync(activity, message);
        }

        #region Private Methods

        private async Task<LogInfo> GetOrCreateLogInfoAsync(IMessageActivity activity)
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
        => await DbContext
            .LogInfo
            .FirstOrDefaultAsync(log => log.ConversationId == activity.Conversation.Id);

        private async Task SaveLogInfoAsync(LogInfo log)
        {
            DbContext.Entry(log).State =
                DbContext.LogInfo.Any(e => e.ConversationId == log.ConversationId) ?
                    EntityState.Modified : EntityState.Added;

            await DbContext.SaveChangesAsync();
        }

        private bool CheckAdmin(IMessageActivity activity)
        {
            var currentConversationId = activity.Conversation.Id;
            var isAdmin = DbContext.MessageInfo.Any(
                messageInfo => messageInfo.IsAdmin &&
                messageInfo.ConversationId == currentConversationId);

            var currentFromId = activity.From.Id;
            var isFromAdmin = DbContext.MessageInfo.Any(messageInfo =>
                    (messageInfo.IsAdmin &&
                    messageInfo.ConversationId == currentFromId &&
                    messageInfo.ToId == currentFromId));

            return isAdmin || isFromAdmin;
        }

        private async Task SendLogAsync(IEnumerable<Core.Log.Models.Log> errorLogs, LogInfo logInfo)
        {
            var filterCategories =
                    logInfo.LogCategories?
                        .Split(';')
                        .Where(category => !string.IsNullOrEmpty(category));

            var groupErrorLogs = errorLogs
                    .OrderBy(log => log.LogId)
                    .GroupBy(log => new { log.CategoryName, log.MachineIP, log.FormattedMessage });

            foreach (var groupErrorLog in groupErrorLogs)
            {
                var errorLog = groupErrorLog.First();
                var logMessage = webLogMessageBuilder.BuildMessage(errorLog);
                var logCategory = errorLog.CategoryName.ToLowerInvariant();
                var hasLogCategory = filterCategories?.Any(
                        filterCategory => logCategory.Contains(filterCategory.ToLowerInvariant())) ?? false;

                var hasIgnoreMessage = await DbContext.LogIgnoreMessage.AnyAsync(
                        message => logCategory.Contains(message.Category.ToLowerInvariant()) &&
                        logMessage.IndexOf(message.IgnoreMessage, StringComparison.InvariantCultureIgnoreCase) >= 0);

                if (hasLogCategory && !hasIgnoreMessage)
                {
                    await Conversation.SendAsync(logInfo.ConversationId, logMessage);
                }
            }
        }

        private async Task SendMissingLogCategoriesMessage(IMessageActivity activity)
            => await Conversation.ReplyAsync(activity, "You need to add [LogCategory], otherwise, you will not get any log info");

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

            var jobId = backgroundJobClient.Schedule(() => RestartNotifyingLog(activity.Conversation.Id), logStopDelayTime);
            cache.Set(
                activity.Conversation.Id,
                jobId,
                new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.NeverRemove
                });
        }

        private void RemoveRestartLogJob(IMessageActivity activity)
        {
            var restartLogJobId = cache.Get<string>(activity.Conversation.Id);

            if (!string.IsNullOrEmpty(restartLogJobId))
            {
                backgroundJobClient.Delete(restartLogJobId);
            }
        }

        #endregion Private Methods
    }
}