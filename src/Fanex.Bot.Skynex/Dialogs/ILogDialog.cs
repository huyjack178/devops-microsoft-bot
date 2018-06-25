namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    public interface ILogDialog : IRootDialog
    {
        Task GetAndSendLogAsync();

        Task RestartNotifyingLog(string conversationId);
    }
}