namespace Fanex.Bot.Skynex.ExecuteSP
{
    using Fanex.Bot.Core._Shared.Constants;
    using Fanex.Bot.Core._Shared.Database;
    using Fanex.Bot.Core.ExecuteSP.Services;
    using Fanex.Bot.Skynex._Shared.Base;
    using Fanex.Bot.Skynex._Shared.MessageSenders;
    using Microsoft.Bot.Connector;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public interface IExecuteSpDialog : IDialog
    {
    }

    public class ExecuteSpDialog : BaseDialog, IExecuteSpDialog
    {
        private readonly IExecuteSpService executeSpService;

        public ExecuteSpDialog(
           BotDbContext dbContext,
           IConversation conversation,
           IExecuteSpService executeSpService)
           : base(dbContext, conversation)
        {
            this.executeSpService = executeSpService;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var regex = new Regex(Regex.Escape(MessageCommand.EXECUTESP));

            var command = regex.Replace(message, string.Empty, 1).Trim();

            var result = await executeSpService.ExecuteSpWithParams(activity.Conversation.Id, command);

            if (!string.IsNullOrEmpty(result.Message))
            {
                await Conversation.ReplyAsync(activity, result.Message);
            }
        }
    }
}