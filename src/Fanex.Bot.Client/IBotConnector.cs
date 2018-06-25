namespace Fanex.Bot.Client
{
    using System.Threading.Tasks;

    public interface IBotConnector
    {
        string Send(string message, string conversationId);
    }
}