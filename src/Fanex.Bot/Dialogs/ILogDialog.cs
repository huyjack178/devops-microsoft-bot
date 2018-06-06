namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;

    public interface ILogDialog
    {
        Task HandleLogMessageAsync(Activity activity, string message);

        Task StartNotifyingLogAsync(Activity activity);

        Task StopNotifyingLogAsync(Activity activity);

        Task AddLogCategoriesAsync(Activity activity, string logCategories);

        Task RemoveLogCategoriesAsync(Activity activity, string logCategories);
    }
}