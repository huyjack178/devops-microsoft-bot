namespace Fanex.Bot.Client.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using Microsoft.Extensions.Configuration;

#pragma warning disable S1075 // URIs should not be hardcoded

    public static class BotSettings
    {
        private static IConfiguration Configuration { get; set; }

        public static void Configure(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static Uri BotServiceUrl { get; internal set; }
            = new Uri(GetValue<string>("FanexBotClient:BotServiceUrl")
                ?? throw new InvalidOperationException("Missing config FanexBotClient:BotServiceUrl"));

        public static string ClientId { get; internal set; }
            = GetValue<string>("FanexBotClient:ClientId")
                ?? throw new InvalidOperationException("Missing config FanexBotClient:ClientId");

        public static string ClientPassword { get; internal set; }
            = GetValue<string>("FanexBotClient:ClientPassword")
            ?? throw new InvalidOperationException("Missing config FanexBotClient:ClientPassword");

        public static Uri TokenUrl { get; internal set; }
             = new Uri(GetValue<string>("FanexBotClient:TokenUrl")
                ?? throw new InvalidOperationException("Missing config FanexBotClient:TokenUrl"));

        private static T GetValue<T>(string key)
        {
            var value = ConfigurationManager.AppSettings[key] ?? Configuration.GetSection(key).Value;

            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T));

            return (T)typeConverter.ConvertFromString(value);
        }
    }

#pragma warning restore S1075 // URIs should not be hardcoded
}