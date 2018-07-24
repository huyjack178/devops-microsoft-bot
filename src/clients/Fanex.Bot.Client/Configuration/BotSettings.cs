namespace Fanex.Bot.Client.Configuration
{
    using System;
    using System.Configuration;

#pragma warning disable S1075 // URIs should not be hardcoded

    public class BotSettings
    {
        public static BotSettings Settings => new BotSettings();

        public static Uri BotServiceUrl { get; internal set; }
            = new Uri(ConfigurationManager.AppSettings["FanexBotClient:BotServiceUrl"] ??
                "https://bot.nexdev.net:6969/skynex/api/");

        public static string ClientId { get; internal set; }
            = ConfigurationManager.AppSettings["FanexBotClient:ClientId"] ??
                "74cab44c-d551-42a7-9bbe-4d460d320516";

        public static string ClientPassword { get; internal set; }
            = ConfigurationManager.AppSettings["FanexBotClient:ClientPassword"] ??
                "qevQN7959^^iaiuNCZUR2@@";

        public static Uri TokenUrl { get; internal set; }
            = new Uri(ConfigurationManager.AppSettings["FanexBotClient:TokenUrl"] ??
                "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token");
    }

#pragma warning restore S1075 // URIs should not be hardcoded
}