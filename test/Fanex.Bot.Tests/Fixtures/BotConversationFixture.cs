namespace Fanex.Bot.Tests.Fixtures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.Bot.Skynex.Models;
    using Fanex.Bot.Skynex.Models.Log;
    using Fanex.Bot.Skynex.Utilities.Bot;
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

        public BotDbContext BotDbContext { get; private set; }

        public IConversation Conversation { get; private set; }

        public IMessageActivity Activity { get; private set; }

        public string CommandMessage { get; }
            = $"Skynex's available commands:{Constants.NewLine}" +
                $"**group** ==> Get your group ID" +
                $"**log add [Contains-LogCategory]** " +
                    $"==> Register to get log which has category name **contains [Contains-LogCategory]**. " +
                    $"Example: log add Alpha;NAP {Constants.NewLine}" +
                $"**log remove [LogCategory]**{Constants.NewLine}" +
                $"**log start** ==> Start receiving logs{Constants.NewLine}" +
                $"**log stop [TimeSpan(Optional)]** ==> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                    $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){Constants.NewLine}" +
                $"**log detail [LogId] (BETA)** ==> Get log detail{Constants.NewLine}" +
                $"**log status** ==> Get your current subscribing Log Categories and Receiving Logs status{Constants.NewLine}" +
                $"**gitlab addProject [GitlabProjectUrl]** => Register to get notification of Gitlab's project{Constants.NewLine}" +
                $"**gitlab removeProject [GitlabProjectUrl]** => Disable getting notification of Gitlab's project{Constants.NewLine}" +
                $"**um start** ==> Start get notification when UM starts {Constants.NewLine}" +
                $"**um addPage [PageUrl]** ==> Add page to check show UM in UM Time. For example: um addPage [http://page1.com;http://page2.com]";

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