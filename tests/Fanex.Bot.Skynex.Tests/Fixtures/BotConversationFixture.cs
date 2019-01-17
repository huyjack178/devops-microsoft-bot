namespace Fanex.Bot.Skynex.Tests.Fixtures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Models.UM;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;

    public class BotConversationFixture : IDisposable
    {
        public BotConversationFixture()
        {
            Configuration = Substitute.For<IConfiguration>();
            Conversation = Substitute.For<IConversation>();
            BotDbContext = MockDbContext();
            Activity = MockActivity();
        }

        public IConfiguration Configuration { get; private set; }

        public BotDbContext BotDbContext { get; }

        public IConversation Conversation { get; private set; }

        public IMessageActivity Activity { get; }

        public string CommandMessage { get; }
            = $"Skynex's available commands:{MessageFormatSignal.NEWLINE} " +
                $"{MessageFormatSignal.BOLD_START}group{MessageFormatSignal.BOLD_END} " +
                    $"=> Get your group ID {MessageFormatSignal.NEWLINE}" + MessageFormatSignal.DIVIDER + MessageFormatSignal.NEWLINE +
                $"{MessageFormatSignal.BOLD_START}log add [Contains-LogCategory]{MessageFormatSignal.BOLD_END} " +
                    $"==> Register to get log which has category name " +
                    $"{MessageFormatSignal.BOLD_START}contains [Contains-LogCategory]{MessageFormatSignal.BOLD_END}. " +
                    $"Example: log add Alpha;NAP {MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}log remove [LogCategory]{MessageFormatSignal.BOLD_END}{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}log start{MessageFormatSignal.BOLD_END} " +
                    $"=> Start receiving logs{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}log stop [TimeSpan(Optional)]{MessageFormatSignal.BOLD_END} " +
                    $"=> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                    $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}log status{MessageFormatSignal.BOLD_END} " +
                    $"=> Get your current subscribing Log Categories and Receiving Logs status{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.DIVIDER}{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}gitlab addProject [GitlabProjectUrl]{MessageFormatSignal.BOLD_END} " +
                    $"=> Register to get notification of Gitlab's project{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}gitlab removeProject [GitlabProjectUrl]{MessageFormatSignal.BOLD_END} " +
                    $"=> Disable getting notification of Gitlab's project{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.DIVIDER}{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}um start{MessageFormatSignal.BOLD_END} " +
                    $"=> Start getting notification when UM starts {MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}um stop{MessageFormatSignal.BOLD_END} " +
                    $"=> Stop getting UM information {MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}um addPage [PageUrl]{MessageFormatSignal.BOLD_END} " +
                    $"=> Add page to check show UM in UM Time. For example: um addPage [http://page1.com;http://page2.com]";

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Configuration = null;
                Conversation = null;
                BotDbContext.MessageInfo = null;
                BotDbContext.GitLabInfo = null;
                BotDbContext.LogInfo = null;
                BotDbContext.LogIgnoreMessage = null;
                BotDbContext.UMInfo = null;
                BotDbContext.UMPage = null;
                BotDbContext.SaveChanges();
            }
        }

        public IMessageActivity MockActivity()
        {
            var activity = Substitute.For<IMessageActivity>();
            activity.From.Returns(new ChannelAccount { Id = "1", Name = "Harrison" });
            activity.Recipient.Returns(new ChannelAccount { Id = "2", Name = "Harrison" });
            activity.Conversation.Returns(new ConversationAccount { Id = "3" });
            activity.ServiceUrl.Returns("http://google.com");
            activity.ChannelId.Returns("skype");

            return activity;
        }

        public BotDbContext MockDbContext(string name = "memcache")
        {
            var builder = new DbContextOptionsBuilder<BotDbContext>().UseInMemoryDatabase(name);
            var botDbContext = new BotDbContext(builder.Options);
            return botDbContext;
        }

        public void InitDbContextData()
        {
            try
            {
                var dbContext = MockDbContext();
                var mesageInfo = CreateMessageInfo();

                foreach (var info in mesageInfo)
                {
                    var existMessageInfo = dbContext.MessageInfo.Any(e => e.ConversationId == info.ConversationId);

                    if (!existMessageInfo)
                    {
                        dbContext.Entry(info).State = EntityState.Added;
                        dbContext.SaveChanges();
                    }
                }

                var logInfo = CreateLogInfo();

                foreach (var info in logInfo)
                {
                    var existLogInfo = dbContext.LogInfo.Any(e => e.ConversationId == info.ConversationId);
                    dbContext.Entry(info).State = existLogInfo ? EntityState.Modified : EntityState.Added;
                    dbContext.SaveChanges();
                }

                var umSites = CreateUMSite();

                foreach (var umSite in umSites)
                {
                    var existLogInfo = dbContext.UMSite.Any(e => e.SiteId == umSite.SiteId);
                    dbContext.Entry(umSite).State = existLogInfo ? EntityState.Modified : EntityState.Added;
                    dbContext.SaveChanges();
                }
            }
            catch
            {
                // Do nothing
            }
        }

        private static List<MessageInfo> CreateMessageInfo()
        {
            return new List<MessageInfo>
            {
                new MessageInfo
                {
                    ConversationId = "1",
                    IsAdmin = true,
                    ToId = "123",
                    ChannelId = "group"
                },
                new MessageInfo
                {
                    ConversationId = "2",
                    IsAdmin = true,
                    ToId = "2",
                    ChannelId = "personal"
                }
            };
        }

        private static List<LogInfo> CreateLogInfo()
        {
            return new List<LogInfo>
            {
                new LogInfo
                {
                    ConversationId = "10",
                    LogCategories = "nap;alpha"
                }
            };
        }

        private static List<UMSite> CreateUMSite()
        {
            return new List<UMSite>
            {
                new UMSite
                {
                    SiteId = "8",
                    SiteName = "Agency"
                },
                 new UMSite
                {
                    SiteId = "2000000",
                    SiteName = "Accounting"
                }
            };
        }
    }
}