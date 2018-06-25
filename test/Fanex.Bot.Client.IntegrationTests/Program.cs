namespace Fanex.Bot.Client.Console
{
#pragma warning disable S1118 // Utility classes should not have public constructors

    internal class Program
    {
        public static void Main(string[] args)
        {
            var botConnector = new BotConnector();
            var result = botConnector.Send(
                "test message from Bot Client Test", "29:1kQdEH4rUnnOBL-xPwgiKBim3BdwtSo1OPgDRK_7WiUU");

            System.Console.WriteLine(result);
            System.Console.ReadLine();
        }
    }

#pragma warning restore S1118 // Utility classes should not have public constructors
}