namespace Fanex.Bot.Tests.Fixtures
{
    using System;
    using Fanex.Bot.Models;
    using Fanex.Bot.Utilitites.Bot;
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
            Activity = MockActivity();
            BotDbContext = MockDbContext();
        }

        public IConfiguration Configuration { get; private set; }

        public BotDbContext BotDbContext { get; private set; }

        public IConversation Conversation { get; private set; }

        public IMessageActivity Activity { get; private set; }

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
                BotDbContext.Dispose();
                Conversation = null;
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

        public BotDbContext MockDbContext()
        {
            var builder = new DbContextOptionsBuilder<BotDbContext>().UseInMemoryDatabase("memcache");
            var botDbContext = new BotDbContext(builder.Options);

            return botDbContext;
        }
    }
}