namespace Fanex.Bot.Client
{
    using System;
    using Fanex.Bot.Client.Configuration;

    public class BotClientManager
    {
        private BotSettings botSettings;
        private static readonly BotClientManager CurrentInstance = new BotClientManager();

        public static BotClientManager UseConfig(BotSettings botSettings)
        {
            CurrentInstance.botSettings = botSettings;

            return CurrentInstance;
        }

        internal static BotSettings BotSettings
            => CurrentInstance.botSettings ?? throw new InvalidOperationException(nameof(CurrentInstance.botSettings));
    }
}