namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.Models;
    using Fanex.Bot.Skynex.Models.UM;
    using Fanex.Bot.Skynex.Services;
    using Fanex.Bot.Tests.Fixtures;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Extensions.Caching.Memory;
    using NSubstitute;
    using Xunit;

    public class UMDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture _conversationFixture;
        private readonly IUMDialog _dialog;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IUMService _umService;
        private readonly IMemoryCache _memoryCache;

        public UMDialogTests(BotConversationFixture conversationFixture)
        {
            _conversationFixture = conversationFixture;
            _recurringJobManager = Substitute.For<IRecurringJobManager>();
            _umService = Substitute.For<IUMService>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _dialog = new UMDialog(_conversationFixture.BotDbContext, conversationFixture.Conversation, _recurringJobManager, _umService, _memoryCache);
        }

        [Fact]
        public async Task HandleMessageAsync_StartUMCommand_ActiveUMInfo_And_AddRecurringJob()
        {
            // Arrange
            var message = "um start";
            _conversationFixture.Activity.Conversation.Id = "1243452df13qqer";

            // Act
            await _dialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.True(_conversationFixture
              .BotDbContext
              .UMInfo
              .FirstOrDefault(info => info.ConversationId == "1243452df13qqer")
              .IsActive);

            _recurringJobManager.Received()
              .AddOrUpdate(
                  Arg.Is("CheckUMPeriodically"),
                  Arg.Any<Job>(),
                  Cron.Minutely(),
                  Arg.Any<RecurringJobOptions>());

            await _conversationFixture.Conversation.Received()
                .ReplyAsync(Arg.Is(_conversationFixture.Activity), "UM notification has been started!");
        }

        [Fact]
        public async Task HandleMessageAsync_AddUMPage_PageIsNotValid_ShowErrorMessage()
        {
            // Arrange
            var message = "um addpage";

            // Act
            await _dialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            await _conversationFixture.Conversation.Received()
               .ReplyAsync(
                    Arg.Is(_conversationFixture.Activity),
                   "You need to input Page Url");
        }

        [Fact]
        public async Task HandleMessageAsync_AddUMPage_SaveUMPage_And_ReplyMessage()
        {
            // Arrange
            var message = "um addpage www.google.com;www.gmail.com";

            // Act
            await _dialog.HandleMessageAsync(_conversationFixture.Activity, message);

            // Assert
            Assert.True(_conversationFixture
             .BotDbContext
             .UMPage
             .Any(page => page.SiteUrl == "www.google.com" && page.IsActive));

            Assert.True(_conversationFixture
             .BotDbContext
             .UMPage
             .Any(page => page.SiteUrl == "www.gmail.com" && page.IsActive));

            await _conversationFixture.Conversation.Received()
               .ReplyAsync(
                    Arg.Is(_conversationFixture.Activity),
                   "Pages will be checked in UM Time\n\n" +
                   "**www.google.com**\n\n" +
                   "**www.gmail.com**\n\n");
        }

        [Fact]
        public async Task CheckUMAsync_IsUM_IsNotInformUM_InformUM()
        {
            // Arrange
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i23749823748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i23749823748923" });
            dbContext.SaveChanges();
            _umService.CheckPageShowUM(Arg.Any<Uri>()).Returns(true);
            _umService.CheckUM().Returns(true);

            // Act
            await _dialog.CheckUMAsync();

            // Assert
            await _conversationFixture.Conversation.Received(1)
              .SendAsync("374i23749823748923", "UM is started now!");

            Assert.True(_memoryCache.Get<bool>("InformedUM"));
        }

        [Fact]
        public async Task CheckUMAsync_IsUM_IsNotInformUM_ScanPageUM_PageNotShowUM_SendMessage()
        {
            // Arrange
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i2374982dfas343748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i2374982dfas343748923" });
            dbContext.UMPage.Add(new UMPage { SiteUrl = "http://www.agbong88.com", Name = "google" });
            dbContext.UMPage.Add(new UMPage { SiteUrl = "http://www.agbong888888.com", Name = "alpha" });
            await dbContext.SaveChangesAsync();

            _umService.CheckUM().Returns(true);
            _umService.CheckPageShowUM(Arg.Is(new Uri("http://www.agbong88.com"))).Returns(false);
            _umService.CheckPageShowUM(Arg.Is(new Uri("http://www.agbong888888.com"))).Returns(true);

            // Act
            await _dialog.CheckUMAsync();

            // Assert
            await _conversationFixture.Conversation.Received()
             .SendAsync("374i2374982dfas343748923", "UM Scanning start");
            await _conversationFixture.Conversation.Received()
                .SendAsync("374i2374982dfas343748923", "**google** ...");
            await _conversationFixture.Conversation.Received()
                .SendAsync("374i2374982dfas343748923", "**http://www.agbong88.com is not in UM**");
            await _conversationFixture.Conversation.Received()
               .SendAsync("374i2374982dfas343748923", "**alpha** ...");
            await _conversationFixture.Conversation.Received()
              .SendAsync("374i2374982dfas343748923", "**alpha** PASSED!");
            await _conversationFixture.Conversation.Received()
               .SendAsync("374i2374982dfas343748923", "----------------------------");
            await _conversationFixture.Conversation.Received()
                .SendAsync("374i2374982dfas343748923", "UM Scanning completed!");
        }

        [Fact]
        public async Task CheckUMAsync_IsNotUM_IsInformedUM_SendFinishUMMessage()
        {
            // Arrange
            var dbContext = _conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i2372342344982dfas343748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i2372342344982dfas343748923" });
            await dbContext.SaveChangesAsync();
            _umService.CheckUM().Returns(false);
            _memoryCache.Set("InformedUM", true);

            // Act
            await _dialog.CheckUMAsync();

            // Assert
            await _conversationFixture.Conversation.Received(1)
             .SendAsync("374i2372342344982dfas343748923", "UM is finished!");
            Assert.False(_memoryCache.Get<bool>("InformedUM"));
        }
    }
}