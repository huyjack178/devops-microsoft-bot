using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Core.UM.Models;
using Fanex.Bot.Core.UM.Services;
using Fanex.Bot.Skynex.UM;
using Fanex.Bot.Skynex.Zabbix;

namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IUnderMaintenanceService umService;
        private readonly IMemoryCache memoryCache;
        private readonly IZabbixDialog zabbixDialog;

        public UMDialogTests(BotConversationFixture conversationFixture)
        {
            this.conversationFixture = conversationFixture;
            recurringJobManager = Substitute.For<IRecurringJobManager>();
            backgroundJobClient = Substitute.For<IBackgroundJobClient>();
            umService = Substitute.For<IUnderMaintenanceService>();
            memoryCache = new MemoryCache(new MemoryCacheOptions());
            zabbixDialog = Substitute.For<IZabbixDialog>();
            this.conversationFixture.Configuration.GetSection("UMInfo").GetSection("UMGMT")?.Value.Returns("8");
            this.conversationFixture.Configuration.GetSection("UMInfo").GetSection("UserGMT")?.Value.Returns("7");
            dialog = new UnderMaintenanceDialog(
                this.conversationFixture.BotDbContext,
                conversationFixture.Conversation,
                recurringJobManager,
                backgroundJobClient,
                umService,
                zabbixDialog,
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
#pragma warning disable S2699 // Tests should include assertions
        public async Task HandleMessageAsync_NotifyUM_SaveUMPage_NotifyToAllUMUser()
#pragma warning restore S2699 // Tests should include assertions
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var message = "um notify";
            var scheduleUnderMaintenanceInfo = new Dictionary<int, Core.UM.Models.UM> { { 8, new Core.UM.Models.UM { From = DateTime.Now, To = DateTime.Now } } };
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
                        $"(GMT +8){MessageFormatSymbol.NEWLINE}" +
                        $"{MessageFormatSymbol.BOLD_START}Site Agency (8){MessageFormatSymbol.BOLD_END} - " +
                        $"From {MessageFormatSymbol.BOLD_START}{scheduleUnderMaintenanceInfo.First().Value.From}{MessageFormatSymbol.BOLD_END} " +
                        $"To {MessageFormatSymbol.BOLD_START}{scheduleUnderMaintenanceInfo.First().Value.From}{MessageFormatSymbol.BOLD_END} {MessageFormatSymbol.NEWLINE}");
        }

        [Fact]
        public async Task CheckUMAsync_IsUM_IsNotInformedUM_InformUM()
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i23749823748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i23749823748923" });
            dbContext.SaveChanges();
            umService.CheckPageShowUM(Arg.Any<Uri>()).Returns(true);
            var underMaintenanceInfo = new Dictionary<int, Core.UM.Models.UM> {
                {
                    8,
                    new Core.UM.Models.UM {
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
                $"Site {MessageFormatSymbol.BOLD_START}Agency (8){MessageFormatSymbol.BOLD_END} is under maintenance now!");

            Assert.True(memoryCache.Get<bool>("InformedUM8"));
        }

        [Fact]
        public async Task CheckUMAsync_IsUM_IsNotInformedUM_ScanPageUM_PageNotShowUM_SendMessage()
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i2374982dfas343748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i2374982dfas343748923", EnableScanPage = true });
            dbContext.UMPage.Add(new UMPage { SiteUrl = "http://www.agbong88.com", Name = "google", SiteId = "8" });
            dbContext.UMPage.Add(new UMPage { SiteUrl = "http://www.agbong888888.com", Name = "alpha", SiteId = "2000000" });
            dbContext.UMPage.Add(new UMPage { SiteUrl = "http://www.agbong8888342388.com", Name = "alpha", SiteId = "2000000" });
            await dbContext.SaveChangesAsync();

            var underMaintenanceInfo = new Dictionary<int, Core.UM.Models.UM> {
                {
                    8,
                    new Core.UM.Models.UM {
                        IsUnderMaintenanceTime = true,
                        From = DateTime.Now,
                        To = DateTime.Now.AddMinutes(90),
                        ConnectionResult = new ConnectionResult { IsOk = true }
                    }
                },
                {
                    2000000,
                    new Core.UM.Models.UM {
                        IsUnderMaintenanceTime = true,
                        From = DateTime.Now,
                        To = DateTime.Now.AddMinutes(90),
                        ConnectionResult = new ConnectionResult { IsOk = true }
                    }
                },
                 {
                    2000001,
                    new Core.UM.Models.UM {
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
                .SendAsync("374i2374982dfas343748923", $"{MessageFormatSymbol.BOLD_START}google{MessageFormatSymbol.BOLD_END} PASSED! {MessageFormatSymbol.NEWLINE}");
            await conversationFixture.Conversation.Received()
               .SendAsync(
                    "374i2374982dfas343748923",
                    $"{MessageFormatSymbol.BOLD_START}alpha{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}" +
                    $"http://www.agbong888888.com does not show UM");
            await conversationFixture.Conversation.DidNotReceive()
              .SendAsync("374i2374982dfas343748923", $"{MessageFormatSymbol.BOLD_START}alpha{MessageFormatSymbol.BOLD_END} PASSED! {MessageFormatSymbol.NEWLINE}");
            await conversationFixture.Conversation.Received(1)
                .SendAsync("374i2374982dfas343748923", "No site to be scanned!");
            await conversationFixture.Conversation.Received()
                .SendAsync("374i2374982dfas343748923", $"Scanning completed!{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.DIVIDER}");
        }

        [Fact]
        public async Task CheckUMAsync_IsNotUM_IsInformedUM_SendFinishUMMessage()
        {
            // Arrange
            conversationFixture.InitDbContextData();
            var dbContext = conversationFixture.MockDbContext();
            dbContext.MessageInfo.Add(new MessageInfo { ConversationId = "374i2372342344982dfas343748923" });
            dbContext.UMInfo.Add(new UMInfo { ConversationId = "374i2372342344982dfas343748923" });
            await dbContext.SaveChangesAsync();
            var underMaintenanceInfo = new Dictionary<int, Core.UM.Models.UM> {
                {
                    8,
                    new Core.UM.Models.UM {
                        IsUnderMaintenanceTime = false,
                        From = DateTime.Now,
                        To = DateTime.Now.AddMinutes(90),
                        ConnectionResult = new ConnectionResult { IsOk = true }
                    }
                }
            };
            umService.GetActualInfo().Returns(underMaintenanceInfo);
            umService.GetScheduledInfo().Returns(underMaintenanceInfo);
            memoryCache.Set("InformedUM8", true);

            // Act
            await dialog.CheckUnderMaintenanceJob();

            // Assert
            await conversationFixture.Conversation.Received(1)
             .SendAsync(
                "374i2372342344982dfas343748923",
                $"Site {MessageFormatSymbol.BOLD_START}Agency (8){MessageFormatSymbol.BOLD_END} is back to normal now!");
            Assert.False(memoryCache.Get<bool>("InformedUM1"));
        }
    }
}