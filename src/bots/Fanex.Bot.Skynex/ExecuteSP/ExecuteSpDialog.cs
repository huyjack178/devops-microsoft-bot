namespace Fanex.Bot.Skynex.ExecuteSP
{
    using System.Threading.Tasks;
    using Fanex.Bot.Core._Shared.Constants;
    using Fanex.Bot.Core._Shared.Database;
    using Fanex.Bot.Core.ExecuteSP.Services;
    using Fanex.Bot.Skynex._Shared.Base;
    using Fanex.Bot.Skynex._Shared.MessageSenders;
    using Microsoft.Bot.Connector;

    public interface IExecuteSpDialog : IDialog
    {
    }

    public class ExecuteSpDialog : BaseDialog, IExecuteSpDialog
    {
        private readonly IExecuteSpService executeSp;

        public ExecuteSpDialog(
           BotDbContext dbContext,
           IConversation conversation,
           IExecuteSpService executeSp)
           : base(dbContext, conversation)
        {
            this.executeSp = executeSp;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var command = message.Replace(MessageCommand.EXECUTESP, string.Empty).Trim();

            var result = await executeSp.ExecuteSpWithParams(command);

            await Conversation.ReplyAsync(activity, result.Message);
        }
    }
}