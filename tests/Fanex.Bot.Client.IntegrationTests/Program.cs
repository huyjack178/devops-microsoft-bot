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

            var message = "<img src=\"http://aka.ms/Fo983c\" alt=\"Duck on a rock\"></img>";

            var result = botConnector.Send(message
                , "29:1CrTamk06NfEFPFPTCv6jnB86xX-fUEdB3Ifwoz6O3iI", Enums.MessageType.XML);

            System.Console.WriteLine(result);

            //botConnector.Send(
            //  "test message from Bot Client Test 2", "29:1VztMrVULRUlh1J7uBBFEWXZqHz41ZRQ6F-avnd5-874");

            System.Console.ReadLine();
        }
    }

#pragma warning restore S1118 // Utility classes should not have public constructors
}