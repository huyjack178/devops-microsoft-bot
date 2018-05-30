namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;

    public interface ILogDialog : IDialog
    {
        Task NotifyLogAsync(ITurnContext context);

        Task RegisterLogCategoryAsync(ITurnContext context, string logCategory);
    }
}