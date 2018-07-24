namespace Fanex.Bot.Client.Console
{
#pragma warning disable S1118 // Utility classes should not have public constructors

    internal class Program
    {
        public static void Main(string[] args)
        {
            var botConnector = new BotConnector();
            var result = botConnector.Send(
                "test message from Bot Client Test", "29:1VztMrVULRUlh1J7uBBFEWXZqHz41ZRQ6F-avnd5-874");

            System.Console.WriteLine(result);

            result = botConnector.Send(
              "test message from Bot Client Test 2", "29:1VztMrVULRUlh1J7uBBFEWXZqHz41ZRQ6F-avnd5-874");

            System.Console.ReadLine();
        }
    }

#pragma warning restore S1118 // Utility classes should not have public constructors
}