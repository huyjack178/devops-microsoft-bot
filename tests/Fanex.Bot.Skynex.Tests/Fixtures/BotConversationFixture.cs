using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Core.Log.Models;
using Fanex.Bot.Core.UM.Models;
using Fanex.Bot.Skynex._Shared.MessageSenders;

namespace Fanex.Bot.Skynex.Tests.Fixtures
{
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

        public IMessageActivity Activity { get; internal set; }

        public string CommandMessage
             => $"Skynex's available commands:{MessageFormatSymbol.NEWLINE} " +
                $"{MessageFormatSymbol.BOLD_START}group{MessageFormatSymbol.BOLD_END} " +
                    $"=> Get your group ID {MessageFormatSymbol.NEWLINE}" +
                    MessageFormatSymbol.DOUBLE_NEWLINE +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} add [Contains-LogCategory]{MessageFormatSymbol.BOLD_END} " +
                    $"==> Register to get log which has category name " +
                    $"{MessageFormatSymbol.BOLD_START}contains [Contains-LogCategory]{MessageFormatSymbol.BOLD_END}. " +
                    $"Example: log add Alpha;NAP {MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} remove [LogCategory]{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} start{MessageFormatSymbol.BOLD_END} " +
                    $"=> Start receiving logs{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} stop [TimeSpan(Optional)]{MessageFormatSymbol.BOLD_END} " +
                    $"=> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                    $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} status{MessageFormatSymbol.BOLD_END} " +
                    $"=> Get your current subscribing Log Categories and Receiving Logs status{MessageFormatSymbol.NEWLINE}" +
                    MessageFormatSymbol.DOUBLE_NEWLINE +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogSentryFunctionName} start{MessageFormatSymbol.BOLD_END} [project_name] level [log_level] " +
                    $"=> Example: log_sentry start nap-api level info{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogSentryFunctionName} stop{MessageFormatSymbol.BOLD_END} [project_name] level [log_level] " +
                    $"=> Example: log_sentry stop nap-api level info{MessageFormatSymbol.NEWLINE}" +
                    MessageFormatSymbol.DOUBLE_NEWLINE +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogDbFunctionName} start{MessageFormatSymbol.BOLD_END} " +
                $"=> Start to get log from Database (for DBA team){MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.DOUBLE_NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}gitlab addProject [GitlabProjectUrl]{MessageFormatSymbol.BOLD_END} " +
                    $"=> Register to get notification of Gitlab's project. " +
                    $"Example: gitlab addProject gitlab.nexdev.net/tools-and-components/ndict {MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}gitlab removeProject [GitlabProjectUrl]{MessageFormatSymbol.BOLD_END} " +
                    $"=> Disable getting notification of Gitlab's project. " +
                    $"Example: gitlab removeProject gitlab.nexdev.net/tools-and-components/ndict {MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.DOUBLE_NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}um start{MessageFormatSymbol.BOLD_END} " +
                    $"=> Start getting notification when UM starts {MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}um stop{MessageFormatSymbol.BOLD_END} " +
                    $"=> Stop getting UM information {MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}um addPage [PageUrl]{MessageFormatSymbol.BOLD_END} " +
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