namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;

    public interface ILogDialog
    {
        Task NotifyLogAsync(ITurnContext context);

        Task NotifyLogPeriodicallyAsync(ITurnContext context);
    }
}