using System;
using System.Configuration;
using System.Net;

namespace Fanex.Bot.Client.Console
{
#pragma warning disable S1118 // Utility classes should not have public constructors

    internal class Program
    {
        public static void Main(string[] args)
        {
            BotClientManager.UseConfig(new Configuration.BotSettings(
                    new Uri(ConfigurationManager.AppSettings["FanexBotClient:BotServiceUrl"]),
                    ConfigurationManager.AppSettings["FanexBotClient:ClientId"],
                    ConfigurationManager.AppSettings["FanexBotClient:ClientPassword"]));

            var botConnector = new BotConnector();

            var message =
                "Missing revision \n Lrf_CustomerSel \n+ 20181031@Patrick: DB052 indexes revolution[RedmineID: #103572]";

            var result = botConnector.Send(message
                , "29:1VztMrVULRUlh1J7uBBFEWXZqHz41ZRQ6F-avnd5-874");

            System.Console.WriteLine(result);

            result = botConnector.Send(
              "test message from Bot Client Test 2", "29:1VztMrVULRUlh1J7uBBFEWXZqHz41ZRQ6F-avnd5-874");

            System.Console.ReadLine();
        }
    }

#pragma warning restore S1118 // Utility classes should not have public constructors
}