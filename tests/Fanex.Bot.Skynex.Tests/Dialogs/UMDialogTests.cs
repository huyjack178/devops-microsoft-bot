namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.UM;
    using Fanex.Bot.Services;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.Tests.Fixtures;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Extensions.Caching.Memory;
    using NSubstitute;
    using Xunit;

    public class UMDialogTests : IClassFixture<BotConversationFixture>
    {
        private readonly BotConversationFixture conversationFixture;
        private readonly IUnderMaintenanceDialog dialog;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IUnderMaintenanceService umService;
        private readonly IMemoryCache memoryCache;

        public UMDialogTests(BotConversationFixture conversationFixture)
        {
            this.conversationFixture = conversationFixture;
            recurringJobManager = Substitute.For<IRecurringJobManager>();
            umService = Substitute.For<IUnderMaintenanceService>();
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            this.conversationFixture.Configuration.GetSection("UMInfo").GetSection("UMGMT")?.Value.Returns("8");
            this.conversationFixture.Configuration.GetSection("UMInfo").GetSection("UserGMT")?.Value.Returns("7");
            dialog = new UnderMaintenanceDialog(
                this.conversationFixture.BotDbContext,
                conversationFixture.Conversation,
                recurringJobManager,
                umService,
                memoryCache,
                this.conversationFixture.Configuration);
        }

        [Fact]
        public async Task HandleMessageAsync_StartUMCommand_ActiveUMInfo_And_AddRecurringJob()
        {
            // Arrange
            var message = "um start";
            conversationFixture.Activity.Conversation.Id = "1243452df13qqer";

            // Act
            await dialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            Assert.True(conversationFixture
                .BotDbContext
                .UMInfo
                .FirstOrDefault(info => info.ConversationId == "1243452df13qqer")
                .IsActive);

            recurringJobManager.Received()
                .AddOrUpdate(
                    Arg.Is("CheckUnderMaintenance"),
                    Arg.Any<Job>(),
                    Cron.Minutely(),
                    Arg.Any<RecurringJobOptions>());

            await conversationFixture.Conversation.Received()
                .ReplyAsync(Arg.Is(conversationFixture.Activity), "Under maintenance notification is enabled!");
        }

        [Fact]
        public async Task HandleMessageAsync_NotifyUM_SaveUMPage_NotifyToAllUMUser()
        {
            // Arrange
            var message = "um notify";
            var scheduleUnderMaintenanceInfo = new Dictionary<int, UM> { { 1, new UM { From = DateTime.Now, To = DateTime.Now } } };
            umService.GetScheduledInfo().Returns(scheduleUnderMaintenanceInfo);
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i324223423342323749823748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i324223423342323749823748923" });
            dbContext.SaveChanges();

            // Act
            await dialog.HandleMessage(conversationFixture.Activity, message);

            // Assert
            await conversationFixture.Conversation
                    .Received(1)
                    .SendAsync(
                        "374i324223423342323749823748923",
                        $"System will be under maintenance with the following information " +
                        $"(GMT +8){MessageFormatSignal.NewLine}" +
                        $"{MessageFormatSignal.BeginBold}SiteId: 1{MessageFormatSignal.EndBold} - " +
                        $"From {MessageFormatSignal.BeginBold}{scheduleUnderMaintenanceInfo.First().Value.From}{MessageFormatSignal.EndBold} " +
                        $"To {MessageFormatSignal.BeginBold}{scheduleUnderMaintenanceInfo.First().Value.From}{MessageFormatSignal.EndBold} {MessageFormatSignal.NewLine}");
        }

        [Fact]
        public async Task CheckUMAsync_IsUM_IsNotInformedUM_InformUM()
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i23749823748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i23749823748923" });
            dbContext.SaveChanges();
            umService.CheckPageShowUM(Arg.Any<Uri>()).Returns(true);
            var underMaintenanceInfo = new Dictionary<int, UM> {
                {
                    1,
                    new UM {
                        IsUnderMaintenanceTime = true,
                        From = DateTime.Now,
                        To = DateTime.Now.AddMinutes(90),
                        ConnectionResult = new ConnectionResult { IsOk = true }
                    }
                }
            };
            umService.GetActualInfo().Returns(underMaintenanceInfo);
            umService.GetScheduledInfo().Returns(underMaintenanceInfo);

            // Act
            await dialog.CheckUnderMaintenanceJob();

            // Assert
            await conversationFixture.Conversation.Received(1)
              .SendAsync(
                "374i23749823748923",
                $"SiteId {MessageFormatSignal.BeginBold}1{MessageFormatSignal.EndBold} is under maintenance now!");

            Assert.True(memoryCache.Get<bool>("InformedUM1"));
        }

        [Fact]
        public async Task CheckUMAsync_IsUM_IsNotInformedUM_ScanPageUM_PageNotShowUM_SendMessage()
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i2374982dfas343748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i2374982dfas343748923", EnableScanPage = true });
            dbContext.UMPage.Add(new UMPage { SiteUrl = "http://www.agbong88.com", Name = "google", SiteId = "1" });
            dbContext.UMPage.Add(new UMPage { SiteUrl = "http://www.agbong888888.com", Name = "alpha", SiteId = "2" });
            dbContext.UMPage.Add(new UMPage { SiteUrl = "http://www.agbong8888342388.com", Name = "alpha", SiteId = "2" });
            await dbContext.SaveChangesAsync();

            var underMaintenanceInfo = new Dictionary<int, UM> {
                {
                    1,
                    new UM {
                        IsUnderMaintenanceTime = true,
                        From = DateTime.Now,
                        To = DateTime.Now.AddMinutes(90),
                        ConnectionResult = new ConnectionResult { IsOk = true }
                    }
                },
                {
                    2,
                    new UM {
                        IsUnderMaintenanceTime = true,
                        From = DateTime.Now,
                        To = DateTime.Now.AddMinutes(90),
                        ConnectionResult = new ConnectionResult { IsOk = true }
                    }
                },
                 {
                    3,
                    new UM {
                        IsUnderMaintenanceTime = true,
                        From = DateTime.Now,
                        To = DateTime.Now.AddMinutes(90),
                        ConnectionResult = new ConnectionResult { IsOk = true }
                    }
                }
            };
            umService.GetActualInfo().Returns(underMaintenanceInfo);
            umService.GetScheduledInfo().Returns(underMaintenanceInfo);
            umService.CheckPageShowUM(Arg.Is(new Uri("http://www.agbong88.com"))).Returns(true);
            umService.CheckPageShowUM(Arg.Is(new Uri("http://www.agbong888888.com"))).Returns(false);
            umService.CheckPageShowUM(Arg.Is(new Uri("http://www.agbong8888342388.com"))).Returns(true);

            // Act
            await dialog.CheckUnderMaintenanceJob();

            // Assert
            await conversationFixture.Conversation.Received()
             .SendAsync("374i2374982dfas343748923", "Scanning started!");
            await conversationFixture.Conversation.Received()
                .SendAsync("374i2374982dfas343748923", $"{MessageFormatSignal.BeginBold}google{MessageFormatSignal.EndBold} PASSED! {MessageFormatSignal.NewLine}");
            await conversationFixture.Conversation.Received()
               .SendAsync(
                    "374i2374982dfas343748923",
                    $"{MessageFormatSignal.BeginBold}alpha{MessageFormatSignal.EndBold}{MessageFormatSignal.NewLine}" +
                    $"http://www.agbong888888.com does not show UM");
            await conversationFixture.Conversation.DidNotReceive()
              .SendAsync("374i2374982dfas343748923", $"{MessageFormatSignal.BeginBold}alpha{MessageFormatSignal.EndBold} PASSED! {MessageFormatSignal.NewLine}");
            await conversationFixture.Conversation.Received(1)
                .SendAsync("374i2374982dfas343748923", "No site to be scanned!");
            await conversationFixture.Conversation.Received()
                .SendAsync("374i2374982dfas343748923", $"Scanning completed!{MessageFormatSignal.NewLine}{MessageFormatSignal.BreakLine}");
        }

        [Fact]
        public async Task CheckUMAsync_IsNotUM_IsInformedUM_SendFinishUMMessage()
        {
            // Arrange
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i2372342344982dfas343748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i2372342344982dfas343748923" });
            await dbContext.SaveChangesAsync();
            var underMaintenanceInfo = new Dictionary<int, UM> {
                {
                    1,
                    new UM {
                        IsUnderMaintenanceTime = false,
                        From = DateTime.Now,
                        To = DateTime.Now.AddMinutes(90),
                        ConnectionResult = new ConnectionResult { IsOk = true }
                    }
                }
            };
            umService.GetActualInfo().Returns(underMaintenanceInfo);
            umService.GetScheduledInfo().Returns(underMaintenanceInfo);
            memoryCache.Set("InformedUM1", true);

            // Act
            await dialog.CheckUnderMaintenanceJob();

            // Assert
            await conversationFixture.Conversation.Received(1)
             .SendAsync(
                "374i2372342344982dfas343748923",
                $"SiteId {MessageFormatSignal.BeginBold}1{MessageFormatSignal.EndBold} is back to normal now!");
            Assert.False(memoryCache.Get<bool>("InformedUM1"));
        }
    }
}