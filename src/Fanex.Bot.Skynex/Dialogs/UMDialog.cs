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

    public interface IUMDialog : IDialog
    {
    }

    public class UMDialog : Dialog, IUMDialog
    {
        private const string InformedUMCacheKey = "InformedUM";
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IUMService _umService;
        private readonly IMemoryCache _memoryCache;

        public UMDialog(
            BotDbContext dbContext,
            IConversation conversation,
            IRecurringJobManager recurringJobManager,
            IUMService umService,
            IMemoryCache memoryCache)
            : base(dbContext, conversation)
        {
            _recurringJobManager = recurringJobManager;
            _umService = umService;
            _memoryCache = memoryCache;
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
            }
        }

        public async Task StartNotifyingUM(IMessageActivity activity)
        {
            var umInfo = await GetOrCreateUMInfoAsync(activity);
            umInfo.IsActive = true;

            await SaveUMInfoAsync(umInfo);

            _recurringJobManager.AddOrUpdate(
                "CheckUMAsync",
                Job.FromExpression(() => CheckUMAsync()),
                Cron.Minutely());

            await Conversation.ReplyAsync(activity, "UM notification has been started!");
        }

        public async Task AddUMPage(IMessageActivity activity, string message)
        {
            var umPageUrls = message
                .Replace(MessageCommand.UM_AddPage, string.Empty)
                .Trim().Split(';');

            if (umPageUrls == null || umPageUrls.Length == 0)
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
            var isUM = await _umService.CheckUM();
            bool informedUM = Convert.ToBoolean(_memoryCache.Get(InformedUMCacheKey) ?? false);

            if (isUM && !informedUM)
            {
                await SendMessageUM("UM is started now!");
                _memoryCache.Set(InformedUMCacheKey, true,
                  new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(1) });

                await ScanPageUM();

                return;
            }

            if (!isUM && informedUM)
            {
                await SendMessageUM("UM is finished!");
                _memoryCache.Remove(InformedUMCacheKey);
            }
        }

        public async Task ScanPageUM()
        {
            var umPageGroup = DbContext.UMPage
                .Where(page => page.IsActive)
                .GroupBy(page => page.Name);

            await SendMessageUM($"UM Scanning start");

            foreach (var group in umPageGroup)
            {
                await SendMessageUM($"**{group.Key}** ...");
                var isShowUM = true;

                foreach (var page in group)
                {
                    isShowUM = await _umService.CheckPageShowUM(new Uri(page.SiteUrl));

                    if (!isShowUM)
                    {
                        await SendMessageUM($"**{page.SiteUrl} is not in UM**");
                    }
                }

                if (isShowUM)
                {
                    await SendMessageUM($"**{group.Key}** PASSED!");
                }

                await SendMessageUM("----------------------------");
            }

            await SendMessageUM($"UM Scanning completed!");
        }

        private async Task SendMessageUM(string message)
        {
            var umMessageInfos = DbContext.UMInfo.Where(info => info.IsActive);

            foreach (var umMessageInfo in umMessageInfos)
            {
                await Conversation.SendAsync(umMessageInfo.ConversationId, message);
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
    }
}