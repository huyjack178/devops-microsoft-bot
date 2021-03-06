using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fanex.Bot.Common.Extensions;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core.UM.Models;
using Fanex.Bot.Core.UM.Services;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Fanex.Bot.Skynex.Zabbix;
using Hangfire;
using Hangfire.Common;
using Microsoft.Bot.Connector;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Fanex.Bot.Skynex.UM
{
    public interface IUnderMaintenanceDialog : IDialog
    {
        Task CheckUnderMaintenanceJob();
    }

    public class UnderMaintenanceDialog : BaseDialog, IUnderMaintenanceDialog
    {
        private const string InformedCacheKey = "InformedUM";
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IUnderMaintenanceService underMaintenanceService;
        private readonly IMemoryCache memoryCache;
        private readonly IZabbixDialog zabbixDialog;
        private readonly int underMaintenanceGMT;
        private readonly int clientGMT;

#pragma warning disable S107 // Methods should not have too many parameters

        public UnderMaintenanceDialog(
            BotDbContext dbContext,
            IConversation conversation,
            IRecurringJobManager recurringJobManager,
            IBackgroundJobClient backgroundJobClient,
            IUnderMaintenanceService umService,
            IZabbixDialog zabbixDialog,
            IMemoryCache memoryCache,
            IConfiguration configuration)
            : base(dbContext, conversation)
        {
            this.recurringJobManager = recurringJobManager;
            this.underMaintenanceService = umService;
            this.memoryCache = memoryCache;
            this.backgroundJobClient = backgroundJobClient;
            this.zabbixDialog = zabbixDialog;

            underMaintenanceGMT = Convert.ToInt32(configuration.GetSection("UMInfo").GetSection("UMGMT").Value ?? "8");
            clientGMT = Convert.ToInt32(configuration.GetSection("UMInfo").GetSection("UserGMT").Value ?? "7");
        }

#pragma warning restore S107 // Methods should not have too many parameters

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var command = message.Replace(MessageCommand.UM, string.Empty).Trim();

            if (command.StartsWith(MessageCommand.START))
            {
                await EnableUnderMaintenanceNotification(activity);
                return;
            }

            if (command.StartsWith(MessageCommand.STOP))
            {
                await DisableUnderMaintenanceNotification(activity);
                return;
            }

            if (command.StartsWith(MessageCommand.UM_START_SCAN))
            {
                await EnableScanUnderMaintenancePage(activity, true);
                return;
            }

            if (command.StartsWith(MessageCommand.UM_STOP_SCAN))
            {
                await EnableScanUnderMaintenancePage(activity, false);
                return;
            }

            if (command.StartsWith(MessageCommand.UM_NOTIFY))
            {
                await NotifyUnderMaintenance();
            }
        }

        public async Task CheckUnderMaintenanceJob()
        {
            var scheduledInfo = await underMaintenanceService.GetScheduledInfo();
            var validScheduledInfo = scheduledInfo.Where(item =>
                    item.Value.ConnectionResult.IsOk
                    && item.Value.From >= DateTime.MinValue
                    && item.Value.To >= DateTime.MinValue);

            var message = BuildScheduledMessage(validScheduledInfo);

            if (!string.IsNullOrEmpty(message))
            {
                await SendMessage(message);
            }

            var actualInfo = await underMaintenanceService.GetActualInfo();

            if (actualInfo.Any(item => item.Value.IsUnderMaintenanceTime))
            {
                recurringJobManager.RemoveIfExists("CheckUnderMaintenance");
                CreateScanServiceJob(actualInfo);
                await StartUnderMaintenanceProcess(actualInfo);
            }

            await EndUnderMaintenanceProcess(actualInfo);
        }

        public async Task ScanServiceJob() => await zabbixDialog.ScanService().ConfigureAwait(false);

        protected async Task EnableUnderMaintenanceNotification(IMessageActivity activity)
        {
            var umInfo = await GetOrCreateUnderMaintenanceInfo(activity);
            umInfo.IsActive = true;

            await SaveUnderMaintenanceInfo(umInfo);

            StartCheckUnderMaintenanceJob();

            await Conversation.ReplyAsync(activity, "Under maintenance notification is enabled!");
        }

        protected async Task EnableScanUnderMaintenancePage(IMessageActivity activity, bool isEnabled)
        {
            var umInfo = await GetOrCreateUnderMaintenanceInfo(activity);
            umInfo.EnableScanPage = isEnabled;

            await SaveUnderMaintenanceInfo(umInfo);

            StartCheckUnderMaintenanceJob();

            var message = isEnabled ? "enabled" : "disable";
            await Conversation.ReplyAsync(activity, $"Scan under maintenance page is {message}!");
        }

        protected async Task DisableUnderMaintenanceNotification(IMessageActivity activity)
        {
            var umInfo = await GetOrCreateUnderMaintenanceInfo(activity);
            umInfo.IsActive = false;

            await SaveUnderMaintenanceInfo(umInfo);
            await Conversation.ReplyAsync(activity, "Under maintenance notification is disabled!");
        }

        protected async Task NotifyUnderMaintenance()
        {
            var umInfo = await underMaintenanceService.GetScheduledInfo();

            var message = BuildScheduledMessage(umInfo, forceNotifyUM: true);

            await SendMessage(message);
        }

        #region Private Methods

        private void StartCheckUnderMaintenanceJob()
        {
            recurringJobManager.AddOrUpdate(
                "CheckUnderMaintenance",
                Job.FromExpression(() => CheckUnderMaintenanceJob()),
                Cron.Minutely());
        }

#pragma warning disable S1541 // Methods and properties should not be too complex

        private string BuildScheduledMessage(IEnumerable<KeyValuePair<int, Core.UM.Models.UM>> underMaintenanceInfos, bool forceNotifyUM = false)
        {
            try
            {
                var now = DateTime.UtcNow.AddHours(clientGMT);
                var underMaintenanceMessage = new StringBuilder(
                        "System will be under maintenance with the following information " +
                        $"(GMT {DateTimeExtention.GenerateGMTText(underMaintenanceGMT)})" +
                        MessageFormatSymbol.NEWLINE);
                var hasValidUnderMaintenanceInfo = false;

                foreach (var item in underMaintenanceInfos)
                {
                    var startTime = item.Value.From.ConvertFromSourceGMTToEndGMT(underMaintenanceGMT, clientGMT);

                    var isAtTenAM = now.Hour == 10 && now.Minute == 0;
                    var isBeforeUnderMaintenanceThirtyMinutes = Convert.ToInt32((startTime - now).TotalMinutes) == 30;
                    var isBeforeUnderMaintenanceOneDayAtTenAM = isAtTenAM && now.Date == startTime.Date.AddDays(-1);
                    var isInUnderMaintenanceDay = (startTime.Date == now.Date) && (isAtTenAM || isBeforeUnderMaintenanceThirtyMinutes);

                    if (forceNotifyUM || isInUnderMaintenanceDay || isBeforeUnderMaintenanceOneDayAtTenAM)
                    {
                        hasValidUnderMaintenanceInfo = true;
                        underMaintenanceMessage
#pragma warning disable S4462 // Calls to "async" methods should not be blocking
                            .Append($"{MessageFormatSymbol.BOLD_START}Site {GetUMSiteName(item.Key).Result} ({item.Key}){MessageFormatSymbol.BOLD_END} - ")
#pragma warning restore S4462 // Calls to "async" methods should not be blocking
                            .Append(
                                $"From {MessageFormatSymbol.BOLD_START}{item.Value.From}{MessageFormatSymbol.BOLD_END} " +
                                $"To {MessageFormatSymbol.BOLD_START}{item.Value.To}{MessageFormatSymbol.BOLD_END} {MessageFormatSymbol.NEWLINE}");
                    }
                }

                return hasValidUnderMaintenanceInfo ? underMaintenanceMessage.ToString() : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

#pragma warning restore S1541 // Methods and properties should not be too complex

        private async Task StartUnderMaintenanceProcess(Dictionary<int, Core.UM.Models.UM> actualInfo)
        {
            foreach (var info in actualInfo.Where(info => info.Value.IsUnderMaintenanceTime))
            {
                var siteInformedCacheKey = InformedCacheKey + info.Key;
                bool hasInformedUnderMaintenanceInfo = Convert.ToBoolean(memoryCache.Get(siteInformedCacheKey) ?? false);

                if (!hasInformedUnderMaintenanceInfo)
                {
#pragma warning disable S109 // Magic numbers should not be used
                    memoryCache.Set(InformedCacheKey + info.Key, true, TimeSpan.FromHours(5));
#pragma warning restore S109 // Magic numbers should not be used
                    await SendMessage($"Site {MessageFormatSymbol.BOLD_START}{await GetUMSiteName(info.Key)} ({info.Key}){MessageFormatSymbol.BOLD_END} is under maintenance now!");
                    await ScanPages(info.Key);
                }
            }

            StartCheckUnderMaintenanceJob();
        }

        private async Task ScanPages(int siteId)
        {
            var umPageGroup = DbContext.UMPage
                .Where(page => page.IsActive && page.SiteId == siteId.ToString())
                .GroupBy(page => page.Name);

            if (!umPageGroup.Any())
            {
                await SendMessage($"No site to be scanned!", isScanPageMessage: true);
                return;
            }

            await SendMessage($"Scanning started!", isScanPageMessage: true);

            foreach (var group in umPageGroup)
            {
                await ScanPageInGroup(group);
            }

            await SendMessage($"Scanning completed!{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.DIVIDER}", isScanPageMessage: true);
        }

        private async Task ScanPageInGroup(IGrouping<string, UMPage> groupPage)
        {
            var allPagesShowUM = true;
            var message = new StringBuilder($"{MessageFormatSymbol.BOLD_START}{groupPage.Key}{MessageFormatSymbol.BOLD_END}");

            foreach (var page in groupPage)
            {
                Uri.TryCreate(page.SiteUrl, UriKind.Absolute, out Uri pageUri);
                var isShowUM = await underMaintenanceService.CheckPageShowUM(pageUri);

                // Rescan
                if (!isShowUM)
                {
                    isShowUM = await underMaintenanceService.CheckPageShowUM(pageUri);
                }

                if (!isShowUM)
                {
                    message.Append($"{MessageFormatSymbol.NEWLINE}{page.SiteUrl} does not show UM");
                    allPagesShowUM = false;
                }
            }

            if (allPagesShowUM)
            {
                message.Append($" PASSED! {MessageFormatSymbol.NEWLINE}");
            }

            await SendMessage(message.ToString(), isScanPageMessage: true);
        }

        private async Task EndUnderMaintenanceProcess(Dictionary<int, Core.UM.Models.UM> actualInfo)
        {
            foreach (var info in actualInfo)
            {
                var siteInformedCacheKey = InformedCacheKey + info.Key;
                bool hasInformedUnderMaintenanceInfo = Convert.ToBoolean(memoryCache.Get(siteInformedCacheKey) ?? false);

                if (hasInformedUnderMaintenanceInfo && !info.Value.IsUnderMaintenanceTime)
                {
                    await SendMessage($"Site {MessageFormatSymbol.BOLD_START}{await GetUMSiteName(info.Key)} ({info.Key}){MessageFormatSymbol.BOLD_END} is back to normal now!");
                    memoryCache.Remove(siteInformedCacheKey);
                }
            }
        }

        private void CreateScanServiceJob(Dictionary<int, Core.UM.Models.UM> actualInfo)
        {
            var jobId = memoryCache.Get<string>("ScanServiceJobId");

            if (string.IsNullOrEmpty(jobId))
            {
                var endUMTime = actualInfo.FirstOrDefault().Value.To.ConvertFromSourceGMTToEndGMT(underMaintenanceGMT, clientGMT);
                var scanServiceTime = endUMTime.AddMinutes(-30);
                jobId = backgroundJobClient.Schedule(() => ScanServiceJob(), scanServiceTime);
                memoryCache.Set("ScanServiceJobId", jobId, endUMTime);
            }
        }

        private async Task<string> GetUMSiteName(int siteId)
            => (await DbContext.UMSite.FirstOrDefaultAsync(site => site.SiteId == siteId.ToString()))?.SiteName;

        private async Task SendMessage(string message, bool isScanPageMessage = false)
        {
            foreach (var info in DbContext.UMInfo.Where(info => info.IsActive))
            {
                if (!isScanPageMessage)
                {
                    await Conversation.SendAsync(info.ConversationId, message).ConfigureAwait(false);
                }

                if (isScanPageMessage && info.EnableScanPage)
                {
                    await Conversation.SendAsync(info.ConversationId, message).ConfigureAwait(false);
                }
            }
        }

        private async Task<UMInfo> GetOrCreateUnderMaintenanceInfo(IMessageActivity activity)
        {
            var underMaintenanceInfo = await GetUnderMaintenanceInfo(activity.Conversation.Id);

            if (underMaintenanceInfo == null)
            {
                underMaintenanceInfo = new UMInfo
                {
                    ConversationId = activity.Conversation.Id,
#pragma warning disable S109 // Magic numbers should not be used
                    CreatedTime = DateTime.UtcNow.AddHours(7)
#pragma warning restore S109 // Magic numbers should not be used
                };
            }

            return underMaintenanceInfo;
        }

        private async Task SaveUnderMaintenanceInfo(UMInfo umInfo)
        {
            var existInfo = (await GetUnderMaintenanceInfo(umInfo.ConversationId)) != null;
            DbContext.Entry(umInfo).State = existInfo ? EntityState.Modified : EntityState.Added;
            await DbContext.SaveChangesAsync();
        }

        private Task<UMInfo> GetUnderMaintenanceInfo(string conversationId)
         => DbContext.UMInfo
             .AsNoTracking()
             .FirstOrDefaultAsync(info => info.ConversationId == conversationId);

        #endregion Private Methods
    }
}