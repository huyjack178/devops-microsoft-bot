namespace Fanex.Bot.Tests.Fixtures
{
    using System;
    using Fanex.Bot.Models;
    using Fanex.Bot.Utilitites.Bot;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;
    using NSubstitute;

    public class BotConversationFixture : IDisposable
    {
        public BotConversationFixture()
        {
            Configuration = Substitute.For<IConfiguration>();
            BotDbContext = Substitute.For<BotDbContext>();
            Conversation = Substitute.For<IConversation>();
            Activity = Substitute.For<IMessageActivity>();
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
    }
}