namespace Fanex.Bot.Client.Models
{
    using System.Configuration;

    public class BotSettings : ConfigurationSection
    {
        public static BotSettings Settings => ConfigurationManager.GetSection("BotSettings") as BotSettings;

        [ConfigurationProperty("BotServiceUrl", IsRequired = true)]
        public string BotServiceUrl => this["BotServiceUrl"].ToString() ?? "https://bot.nexdev.net:6969/api/";

        [ConfigurationProperty("ClientId", IsRequired = true)]
        public string ClientId => this["ClientId"].ToString() ?? "74cab44c-d551-42a7-9bbe-4d460d320516";

        [ConfigurationProperty("ClientPassword", IsRequired = true)]
        public string ClientPassword => this["ClientPassword"].ToString() ?? "qevQN7959^^iaiuNCZUR2@@";

        [ConfigurationProperty("TokenUrl", IsRequired = true)]
        public string TokenUrl => this["TokenUrl"].ToString() ?? "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token";
    }
}