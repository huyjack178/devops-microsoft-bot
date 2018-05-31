namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;

    public interface ILogDialog
    {
        Task HandleLogMessageAsync(ITurnContext context, string message);

        Task StartNotifyingLogAsync(ITurnContext context);

        Task StopNotifyingLogAsync(ITurnContext context);

        Task AddLogCategoriesAsync(ITurnContext context, string logCategories);

        Task RemoveLogCategoriesAsync(ITurnContext context, string logCategories);
    }
}