namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Connector;

    public interface IDialog
    {
        Task SendAsync(MessageInfo messageInfo);

        Task SendAsync(Activity activity, string message, bool notifyAdmin = true);

        Task RegisterMessageInfo(Activity activity);
    }
}