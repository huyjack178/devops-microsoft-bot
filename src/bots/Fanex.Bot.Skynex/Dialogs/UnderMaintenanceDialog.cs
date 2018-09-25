namespace Fanex.Bot.Skynex.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Common;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.UM;
    using Fanex.Bot.Services;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;

    public interface IUnderMaintenanceDialog : IDialog
    {
        Task CheckUnderMaintenance();
    }

    public class UnderMaintenanceDialog : BaseDialog, IUnderMaintenanceDialog
    {
        private const string InformedCacheKey = "InformedUM";
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IUnderMaintenanceService underMaintenanceService;
        private readonly IMemoryCache memoryCache;
        private readonly IConfiguration configuration;

        public UnderMaintenanceDialog(
            BotDbContext dbContext,
            IConversation conversation,
            IRecurringJobManager recurringJobManager,
            IUnderMaintenanceService umService,
            IMemoryCache memoryCache,
            IConfiguration configuration)
            : base(dbContext, conversation)
        {
            this.recurringJobManager = recurringJobManager;
            this.underMaintenanceService = umService;
            this.memoryCache = memoryCache;
            this.configuration = configuration;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var command = message.Replace(MessageCommand.UM, string.Empty).Trim();

            if (command.StartsWith(MessageCommand.Start))
            {
                await EnableUnderMaintenanceNotification(activity);
                return;
            }

            if (command.StartsWith(MessageCommand.Stop))
            {
                await DisableUnderMaintenanceNotification(activity);
                return;
            }

            if (command.StartsWith(MessageCommand.UM_Notify))
            {
                await NotifyUnderMaintenance();
            }
        }

        public async Task CheckUnderMaintenance()
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
                await StartUnderMaintenanceProcess(actualInfo);
            }

            await EndUnderMaintenanceProcess(actualInfo);
        }

        protected async Task EnableUnderMaintenanceNotification(IMessageActivity activity)
        {
            var umInfo = await GetOrCreateUnderMaintenanceInfo(activity);
            umInfo.IsActive = true;

            await SaveUnderMaintenanceInfo(umInfo);

            StartCheckUnderMaintenanceJob();

            await Conversation.ReplyAsync(activity, "Under maintenance notification is enabled!");
        }

        private void StartCheckUnderMaintenanceJob()
        {
            recurringJobManager.AddOrUpdate(
                "CheckUnderMaintenance",
                Job.FromExpression(() => CheckUnderMaintenance()),
                Cron.Minutely());
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

#pragma warning disable S1541 // Methods and properties should not be too complex

        private string BuildScheduledMessage(IEnumerable<KeyValuePair<int, UM>> underMaintenanceInfos, bool forceNotifyUM = false)
        {
            try
            {
                var underMaintenanceGMT = Convert.ToInt32(configuration.GetSection("UMInfo").GetSection("UMGMT").Value ?? "8");
                var clientGMT = Convert.ToInt32(configuration.GetSection("UMInfo").GetSection("UserGMT").Value ?? "7");
                var now = DateTime.UtcNow.AddHours(clientGMT);
                var underMaintenanceMessage = new StringBuilder(
                        "System will be under maintenance with the following information " +
                        $"(GMT {DateTimeExtention.GenerateGMTText(underMaintenanceGMT)})" +
                        MessageFormatSignal.NewLine);
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
                            .Append($"{MessageFormatSignal.BeginBold}SiteId: {item.Key}{MessageFormatSignal.EndBold} - ")
                            .Append(
                                $"From {MessageFormatSignal.BeginBold}{item.Value.From}{MessageFormatSignal.EndBold} " +
                                $"To {MessageFormatSignal.BeginBold}{item.Value.To}{MessageFormatSignal.EndBold} {MessageFormatSignal.NewLine}");
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

        private async Task StartUnderMaintenanceProcess(Dictionary<int, UM> actualInfo)
        {
            var underMaintenanceInfo = actualInfo.Where(info => info.Value.IsUnderMaintenanceTime);

            foreach (var info in underMaintenanceInfo)
            {
                var siteInformedCacheKey = InformedCacheKey + info.Key;
                bool hasInformedUnderMaintenanceInfo = Convert.ToBoolean(memoryCache.Get(siteInformedCacheKey) ?? false);

                if (!hasInformedUnderMaintenanceInfo)
                {
                    memoryCache.Set(InformedCacheKey + info.Key, true, TimeSpan.FromHours(5));
                    await SendMessage($"SiteId {MessageFormatSignal.BeginBold}{info.Key}{MessageFormatSignal.EndBold} is under maintenance now!");
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
                await SendMessage($"No site to be scanned!");
                return;
            }

            await SendMessage($"Scanning started!");

            foreach (var group in umPageGroup)
            {
                await ScanPageInGroup(group);
            }

            await SendMessage($"Scanning completed!{MessageFormatSignal.NewLine}{MessageFormatSignal.BreakLine}");
        }

        private async Task ScanPageInGroup(IGrouping<string, UMPage> groupPage)
        {
            var allPagesShowUM = true;
            var message = new StringBuilder($"{MessageFormatSignal.BeginBold}{groupPage.Key}{MessageFormatSignal.EndBold}");

            foreach (var page in groupPage)
            {
                Uri.TryCreate(page.SiteUrl, UriKind.Absolute, out Uri pageUri);
                var isShowUM = await underMaintenanceService.CheckPageShowUM(pageUri);

                if (!isShowUM)
                {
                    message.Append($"{MessageFormatSignal.NewLine}{page.SiteUrl} does not show UM");
                    allPagesShowUM = false;
                }
            }

            if (allPagesShowUM)
            {
                message.Append($" PASSED! {MessageFormatSignal.NewLine}");
            }

            await SendMessage(message.ToString());
        }

        private async Task EndUnderMaintenanceProcess(Dictionary<int, UM> actualInfo)
        {
            foreach (var info in actualInfo)
            {
                var siteInformedCacheKey = InformedCacheKey + info.Key;
                bool hasInformedUnderMaintenanceInfo = Convert.ToBoolean(memoryCache.Get(siteInformedCacheKey) ?? false);

                if (hasInformedUnderMaintenanceInfo && !info.Value.IsUnderMaintenanceTime)
                {
                    await SendMessage($"SiteId {MessageFormatSignal.BeginBold}{info.Key}{MessageFormatSignal.EndBold} is back to normal now!");
                    memoryCache.Remove(siteInformedCacheKey);
                }
            }
        }

        private async Task SendMessage(string message)
        {
            foreach (var info in DbContext.UMInfo.Where(info => info.IsActive))
            {
                await Conversation.SendAsync(info.ConversationId, message).ConfigureAwait(false);
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
                    CreatedTime = DateTime.UtcNow.AddHours(7)
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