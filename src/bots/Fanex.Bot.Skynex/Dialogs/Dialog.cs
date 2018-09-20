namespace Fanex.Bot.Skynex.Dialogs
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Microsoft.EntityFrameworkCore;
    using Connector = Microsoft.Bot.Connector;

    public interface IDialog
    {
        Task HandleMessage(Connector.IMessageActivity activity, string message);
    }

    public class Dialog : IDialog
    {
        public Dialog(
            BotDbContext dbContext,
            IConversation conversation)
        {
            DbContext = dbContext;
            Conversation = conversation;
        }

        public IConversation Conversation { get; }

        public BotDbContext DbContext { get; }

        public static string GetCommandMessages()
            => $"Skynex's available commands:{MessageFormatSignal.NewLine} " +
                $"{MessageFormatSignal.BeginBold}group{MessageFormatSignal.EndBold} " +
                    $"=> Get your group ID {MessageFormatSignal.NewLine}" + MessageFormatSignal.BreakLine + MessageFormatSignal.NewLine +
                $"{MessageFormatSignal.BeginBold}log add [Contains-LogCategory]{MessageFormatSignal.EndBold} " +
                    $"==> Register to get log which has category name " +
                    $"{MessageFormatSignal.BeginBold}contains [Contains-LogCategory]{MessageFormatSignal.EndBold}. " +
                    $"Example: log add Alpha;NAP {MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}log remove [LogCategory]{MessageFormatSignal.EndBold}{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}log start{MessageFormatSignal.EndBold} " +
                    $"=> Start receiving logs{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}log stop [TimeSpan(Optional)]{MessageFormatSignal.EndBold} " +
                    $"=> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                    $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}log status{MessageFormatSignal.EndBold} " +
                    $"=> Get your current subscribing Log Categories and Receiving Logs status{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BreakLine}{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}gitlab addProject [GitlabProjectUrl]{MessageFormatSignal.EndBold} " +
                    $"=> Register to get notification of Gitlab's project{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}gitlab removeProject [GitlabProjectUrl]{MessageFormatSignal.EndBold} " +
                    $"=> Disable getting notification of Gitlab's project{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BreakLine}{MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}um start{MessageFormatSignal.EndBold} " +
                    $"=> Start get notification when UM starts {MessageFormatSignal.NewLine}" +
                $"{MessageFormatSignal.BeginBold}um addPage [PageUrl]{MessageFormatSignal.EndBold} " +
                    $"=> Add page to check show UM in UM Time. For example: um addPage [http://page1.com;http://page2.com]";

        public virtual Task HandleMessage(Connector.IMessageActivity activity, string message)
        {
            throw new System.NotImplementedException();
        }

        protected async Task SaveMessageInfo(MessageInfo messageInfo)
        {
            var existMessageInfo = await ExistMessageInfo(messageInfo);
            DbContext.Entry(messageInfo).State = existMessageInfo ? EntityState.Modified : EntityState.Added;

            await DbContext.SaveChangesAsync();
        }

        private Task<bool> ExistMessageInfo(MessageInfo messageInfo)
            => DbContext.MessageInfo.AnyAsync(e => e.ConversationId == messageInfo.ConversationId);
    }
}