namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Fanex.Bot.Utilitites.Bot;
    using Microsoft.Bot.Connector;

    public interface IDialog
    {
        IConversation Conversation { get; }

        Task RegisterMessageInfo(IMessageActivity activity);

        Task RemoveConversationData(IMessageActivity activity);
    }
}