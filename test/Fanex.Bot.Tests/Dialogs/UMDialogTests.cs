namespace Fanex.Bot.Skynex.Tests.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.Services;
    using Fanex.Bot.Tests.Fixtures;
    using Hangfire;
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
        public async Task HandleMessageAsync_StartUMCommand_ActiveUMInfo()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}