namespace Fanex.Bot.Skynex.Dialogs
{
    using System;
    using System.Collections.Generic;
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
            const string informedUMCacheKey = "InformedUM";
            bool informedUM = Convert.ToBoolean(_memoryCache.Get(informedUMCacheKey) ?? false);
            var umMessageInfos = DbContext.UMInfo.Where(info => info.IsActive);

            if (isUM && !informedUM)
            {
                await InformStartUM(umMessageInfos);
                _memoryCache.Set(
                      informedUMCacheKey,
                      true,
                      new MemoryCacheEntryOptions
                      {
                          SlidingExpiration = TimeSpan.FromHours(1)
                      });

                await ScanAllPageShowUM(umMessageInfos);
                return;
            }

            if (!isUM && informedUM)
            {
                await InformStopUM(umMessageInfos);
                _memoryCache.Remove(informedUMCacheKey);
            }
        }

        public async Task ScanAllPageShowUM(IQueryable<UMInfo> umMessageInfos)
        {
            var pagesNotShowUM = await CheckAndGetPagesNotShowUM();

            foreach (var umMessageInfo in umMessageInfos)
            {
                if (pagesNotShowUM.Any())
                {
                    var pageListMessage = new StringBuilder();

                    foreach (var page in pagesNotShowUM)
                    {
                        pageListMessage.Append($"**{page}** {Constants.NewLine}");
                    }

                    await Conversation.SendAsync(
                        umMessageInfo.ConversationId,
                        $"Pages have not shown UM yet!" +
                        $"{Constants.NewLine}{pageListMessage.ToString()} ");
                }
                else
                {
                    await Conversation.SendAsync(umMessageInfo.ConversationId, "All pages show UM now!");
                }
            }
        }

        private async Task<List<string>> CheckAndGetPagesNotShowUM()
        {
            var pagesNotShowUM = new List<string>();

            foreach (var page in DbContext.UMPage)
            {
                var isShowUM = await _umService.CheckPageShowUM(new Uri(page.SiteUrl));

                if (!isShowUM)
                {
                    pagesNotShowUM.Add(page.SiteUrl);
                }
            }

            return pagesNotShowUM;
        }

        private async Task InformStartUM(IQueryable<UMInfo> umMessageInfos)
        {
            foreach (var umMessageInfo in umMessageInfos)
            {
                await Conversation.SendAsync(umMessageInfo.ConversationId, "UM is started now!");
            }
        }

        private async Task InformStopUM(IQueryable<UMInfo> umMessageInfos)
        {
            foreach (var umMessageInfo in umMessageInfos)
            {
                await Conversation.SendAsync(umMessageInfo.ConversationId, "UM is stopped now!");
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