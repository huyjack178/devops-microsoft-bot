namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Builder;

    public interface IDialog
    {
        Task SendAsync(MessageInfo messageInfo);

        Task RegisterConversation(ITurnContext context);
    }
}