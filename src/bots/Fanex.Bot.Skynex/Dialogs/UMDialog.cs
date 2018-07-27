namespace Fanex.Bot.Skynex.Dialogs
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Bot;
    using Fanex.Bot.Skynex.Models;
    using Fanex.Bot.Skynex.Models.UM;
    using Fanex.Bot.Skynex.Services;
    using Fanex.Bot.Skynex.Utilities.Bot;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Fanex.Bot.Core.Utilities.Common;

    public interface IUMDialog : IDialog
    {
        Task CheckUMAsync();
    }

    public class UMDialog : Dialog, IUMDialog
    {
        private const string InformedUMCacheKey = "InformedUM";
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IUMService _umService;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        public UMDialog(
            BotDbContext dbContext,
            IConversation conversation,
            IRecurringJobManager recurringJobManager,
            IUMService umService,
            IMemoryCache memoryCache,
            IConfiguration configuration)
            : base(dbContext, conversation)
        {
            _recurringJobManager = recurringJobManager;
            _umService = umService;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async override Task HandleMessageAsync(IMessageActivity activity, string message)
        {
            var command = message.Replace(MessageCommand.UM, string.Empty).Trim();

            if (command.StartsWith(MessageCommand.Start))
            {
                await StartNotifyingUM(activity);
                return;
            }

            if (command.StartsWith(MessageCommand.UM_AddPage))
            {
                await AddUMPage(activity, command);
                return;
            }

            if (command.StartsWith(MessageCommand.UM_Notify))
            {
                await NotifyUMAsync();
            }
        }

        public async Task StartNotifyingUM(IMessageActivity activity)
        {
            var umInfo = await GetOrCreateUMInfoAsync(activity);
            umInfo.IsActive = true;

            await SaveUMInfoAsync(umInfo);

            _recurringJobManager.AddOrUpdate(
                "CheckUMPeriodically",
                Job.FromExpression(() => CheckUMAsync()),
                Cron.Minutely());

            await Conversation.ReplyAsync(activity, "UM notification has been started!");
        }

        public async Task AddUMPage(IMessageActivity activity, string message)
        {
            var umPageUrls = message
                .Replace(MessageCommand.UM_AddPage, string.Empty)
                .Trim().Split(';');

            if (umPageUrls == null || umPageUrls.Length == 0 || umPageUrls[0] == string.Empty)
            {
                await Conversation.ReplyAsync(
                    activity,
                    $"You need to input Page Url");
                return;
            }

            var umPageUrlMessage = new StringBuilder();

            foreach (var umPageUrl in umPageUrls)
            {
                var processedUmPageUrl = BotHelper.ExtractProjectLink(umPageUrl);
                var umPages = DbContext.UMPage;
                var existUMPage = await umPages.AnyAsync(
                        umPage => umPage.SiteUrl.ToLowerInvariant() == processedUmPageUrl.ToLowerInvariant());

                if (!existUMPage)
                {
                    await umPages.AddAsync(new UMPage { SiteUrl = processedUmPageUrl });
                }

                umPageUrlMessage.Append($"**{umPageUrl}**{Constants.NewLine}");
            }

            await DbContext.SaveChangesAsync();

            await Conversation.ReplyAsync(
                activity,
                $"Pages will be checked in UM Time" +
                $"{Constants.NewLine}{umPageUrlMessage.ToString()}");
        }

        public async Task CheckUMAsync()
        {
            var umInfo = await _umService.GetUMInformation();
            bool informedUM = Convert.ToBoolean(_memoryCache.Get(InformedUMCacheKey) ?? false);

            await SendUMInformation(umInfo);
            await SendUMStartMessage(umInfo, informedUM);
            await SendUMFinisedMessage(umInfo, informedUM);
        }

        public async Task NotifyUMAsync()
        {
            var umInfo = await _umService.GetUMInformation();

            await SendUMInformation(umInfo, forceNotifyUM: true);
        }

        #region Private Methods

        private async Task SendUMFinisedMessage(UM umInfo, bool informedUM)
        {
            if (!umInfo.IsUM && informedUM)
            {
                await SendMessageUM("UM is finished!");
                _memoryCache.Remove(InformedUMCacheKey);
            }
        }

        private async Task SendUMStartMessage(UM umInfo, bool informedUM)
        {
            if (umInfo.IsUM && !informedUM)
            {
                await SendMessageUM("UM is started now!");
                _memoryCache.Set(InformedUMCacheKey, true, TimeSpan.FromHours(2));
                await ScanPages();
            }
        }

        private async Task SendUMInformation(UM umInfo, bool forceNotifyUM = false)
        {
            if (umInfo.ErrorCode != 0)
            {
                return;
            }

            var umGMT = Convert.ToInt32(_configuration.GetSection("UMInfo").GetSection("UMGMT").Value ?? "8");
            var userGMT = Convert.ToInt32(_configuration.GetSection("UMInfo").GetSection("UserGMT").Value ?? "7");
            var umStartTime = umInfo.StartTime.ConvertFromSourceGMTToEndGMT(umGMT, userGMT);
            var now = DateTimeExtention.GetUTCNow().AddHours(userGMT);

            await CheckTimeAndSendUMInfo(umInfo, forceNotifyUM, umGMT, umStartTime, now);
        }

        private async Task CheckTimeAndSendUMInfo(UM umInfo, bool forceNotifyUM, int umGMT, DateTime umStartTime, DateTime now)
        {
            var isInUMDate = umInfo.StartTime.Date == now.Date;
            var isAt10AM = now.Hour == 10 && now.Minute == 0;
            var isBefore30Mins = Convert.ToInt32((umStartTime - now).TotalMinutes) == 30;
            var isBefore1DayAt10AM = isAt10AM && now.Date == umInfo.StartTime.Date.AddDays(-1);
            var isInDay = isInUMDate && (isAt10AM || isBefore30Mins);

            if (forceNotifyUM || isInDay || isBefore1DayAt10AM)
            {
                await SendMessageUM(
                    $"System will be **under maintenance** " +
                    $"from **{umInfo.StartTime}** to **{umInfo.EndTime}** " +
                    $"**(GMT{DateTimeExtention.GenerateGMTText(umGMT)})**");
            }
        }

        private async Task ScanPages()
        {
            var umPageGroup = DbContext.UMPage
                .Where(page => page.IsActive)
                .GroupBy(page => page.Name);

            await SendMessageUM($"UM Scanning started!");

            foreach (var group in umPageGroup)
            {
                await ScanPageInGroup(group);
            }

            await SendMessageUM($"UM Scanning completed!");
        }

        private async Task ScanPageInGroup(IGrouping<string, UMPage> groupPage)
        {
            var allPagesShowUM = true;
            var message = new StringBuilder($"**{groupPage.Key}**");

            foreach (var page in groupPage)
            {
                Uri.TryCreate(page.SiteUrl, UriKind.Absolute, out Uri pageUri);
                var isShowUM = await _umService.CheckPageShowUM(pageUri);

                if (!isShowUM)
                {
                    message.Append($"{Constants.NewLine}**{page.SiteUrl} does not show UM**");
                    allPagesShowUM = false;
                }
            }

            if (allPagesShowUM)
            {
                message.Append($" PASSED! {Constants.NewLine}");
            }

            await SendMessageUM(message.ToString());
        }

        private async Task SendMessageUM(string message)
        {
            var umInfos = DbContext.UMInfo.Where(info => info.IsActive);

            foreach (var info in umInfos)
            {
                await Conversation.SendAsync(info.ConversationId, message);
            }
        }

        private async Task<UMInfo> GetOrCreateUMInfoAsync(IMessageActivity activity)
        {
            var umInfo = await GetUMInfo(activity.Conversation.Id);

            if (umInfo == null)
            {
                umInfo = new UMInfo
                {
                    ConversationId = activity.Conversation.Id,
                    CreatedTime = DateTime.UtcNow.AddHours(7)
                };
            }

            return umInfo;
        }

        private async Task SaveUMInfoAsync(UMInfo umInfo)
        {
            var existInfo = (await GetUMInfo(umInfo.ConversationId)) != null;
            DbContext.Entry(umInfo).State = existInfo ? EntityState.Modified : EntityState.Added;
            await DbContext.SaveChangesAsync();
        }

        private async Task<UMInfo> GetUMInfo(string conversationId)
         => await DbContext.UMInfo
             .AsNoTracking()
             .FirstOrDefaultAsync(info => info.ConversationId == conversationId);

        #endregion Private Methods
    }
}