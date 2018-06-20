namespace Fanex.Bot.Utilitites.Bot
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Connector;

    public interface IConversation
    {
        Task SendAsync(IMessageActivity activity, string message);

        Task SendAsync(MessageInfo messageInfo);

        Task<string> SendAsync(string conversationId, string message);

        Task SendAdminAsync(string message);

        ConnectorClient CreateConnectorClient(Uri serviceUrl);
    }
}