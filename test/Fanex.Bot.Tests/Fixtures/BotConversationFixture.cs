namespace Fanex.Bot.Skynex.Tests.Fixtures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
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
            = $"Skynex's available commands:{MessageFormatSignal.NewLine} " +
                $"{MessageFormatSignal.BeginBold}group{MessageFormatSignal.EndBold} " +
                    $"=> Get your group ID {MessageFormatSignal.NewLine}" + MessageFormatSignal.BreakLine + MessageFormatSignal.NewLine +
                $"{MessageFormatSignal.BeginBold}log add [Contains-LogCategory]{MessageFormatSignal.EndBold} " +
                    $"==> Register to get log which has category name " +
                    $"{MessageFormatSignal.BeginBold}contains [Contains-LogCategory]{MessageFormatSignal.EndBold}. " +
                    $"Example: log add Alpha;NAP {MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}log remove [LogCategory]{MessageFormatSignal.EndBold}{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}log start{MessageFormatSignal.EndBold} " +
                    $"=> Start receiving logs{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}log stop [TimeSpan(Optional)]{MessageFormatSignal.EndBold} " +
                    $"=> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                    $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}log status{MessageFormatSignal.EndBold} " +
                    $"=> Get your current subscribing Log Categories and Receiving Logs status{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BreakLine}{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}gitlab addProject [GitlabProjectUrl]{MessageFormatSignal.EndBold} " +
                    $"=> Register to get notification of Gitlab's project{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}gitlab removeProject [GitlabProjectUrl]{MessageFormatSignal.EndBold} " +
                    $"=> Disable getting notification of Gitlab's project{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BreakLine}{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}um start{MessageFormatSignal.EndBold} " +
                    $"=> Start getting notification when UM starts {MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}um stop{MessageFormatSignal.EndBold} " +
                    $"=> Stop getting UM information {MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}um addPage [PageUrl]{MessageFormatSignal.EndBold} " +
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
            var dbContext = MockDbContext();
            var mesageInfo = CreateMessageInfo();

            foreach (var info in mesageInfo)
            {
                var existMessageInfo = dbContext.MessageInfo.Any(e => e.ConversationId == info.ConversationId);
                dbContext.Entry(info).State = existMessageInfo ? EntityState.Modified : EntityState.Added;
                dbContext.SaveChanges();
            }

            var logInfo = CreateLogInfo();

            foreach (var info in logInfo)
            {
                var existLogInfo = dbContext.LogInfo.Any(e => e.ConversationId == info.ConversationId);
                dbContext.Entry(info).State = existLogInfo ? EntityState.Modified : EntityState.Added;
                dbContext.SaveChanges();
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
    }
}