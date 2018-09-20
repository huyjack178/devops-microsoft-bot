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
        {
            return $"Skynex's available commands:{MessageFormatSignal.NewLine} " +
                    $"------------- {MessageFormatSignal.NewLine}" +
                    $"**group** ==> Get your group ID {MessageFormatSignal.NewLine}" +
                    $"------------- {MessageFormatSignal.NewLine}" +
                    $"**log add [Contains-LogCategory]** " +
                        $"==> Register to get log which has category name **contains [Contains-LogCategory]**. " +
                        $"Example: log add Alpha;NAP {MessageFormatSignal.NewLine}" +
                    $"**log remove [LogCategory]**{MessageFormatSignal.NewLine}" +
                    $"**log start** ==> Start receiving logs{MessageFormatSignal.NewLine}" +
                    $"**log stop [TimeSpan(Optional)]** ==> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                        $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){MessageFormatSignal.NewLine}" +
                    $"**log detail [LogId] (BETA)** ==> Get log detail{MessageFormatSignal.NewLine}" +
                    $"**log status** ==> Get your current subscribing Log Categories and Receiving Logs status{MessageFormatSignal.NewLine}" +
                    $"------------- {MessageFormatSignal.NewLine}" +
                    $"**gitlab addProject [GitlabProjectUrl]** => Register to get notification of Gitlab's project{MessageFormatSignal.NewLine}" +
                    $"**gitlab removeProject [GitlabProjectUrl]** => Disable getting notification of Gitlab's project{MessageFormatSignal.NewLine}" +
                    $"------------- {MessageFormatSignal.NewLine}" +
                    $"**um start** ==> Start get notification when UM starts {MessageFormatSignal.NewLine}" +
                    $"**um addPage [PageUrl]** ==> Add page to check show UM in UM Time. For example: um addPage [http://page1.com;http://page2.com]";
        }

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