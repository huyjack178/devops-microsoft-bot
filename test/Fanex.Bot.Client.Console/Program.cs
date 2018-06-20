namespace Fanex.Bot.Client.Console
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var botConnector = new BotConnector();
            var result = botConnector.SendAsync("hello jack", "29:1VztMrVULRUlh1J7uBBFEWXZqHz41ZRQ6F-avnd5-874").Result;

            System.Console.WriteLine(result);
            System.Console.ReadLine();
        }
    }
}