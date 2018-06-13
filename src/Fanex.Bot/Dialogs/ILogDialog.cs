namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    public interface ILogDialog : IRootDialog
    {
        Task StartNotifyingLogAsync(IMessageActivity activity);

        Task StopNotifyingLogAsync(IMessageActivity activity);

        Task AddLogCategoriesAsync(IMessageActivity activity, string messageCmd);

        Task RemoveLogCategoriesAsync(IMessageActivity activity, string messageCmd);
    }
}