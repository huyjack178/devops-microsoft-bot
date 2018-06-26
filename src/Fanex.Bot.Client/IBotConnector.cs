namespace Fanex.Bot.Client
{
    public interface IBotConnector
    {
        string Send(string message, string conversationId);
    }
}