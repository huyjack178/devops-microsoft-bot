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

    public class BaseDialog
    {
        public BaseDialog(
            BotDbContext dbContext,
            IConversation conversation)
        {
            DbContext = dbContext;
            Conversation = conversation;
        }

        public IConversation Conversation { get; }

        public BotDbContext DbContext { get; }

        public static string GetCommandMessages()
            => $"Skynex's available commands:{MessageFormatSignal.NEWLINE} " +
                $"{MessageFormatSignal.BOLD_START}group{MessageFormatSignal.BOLD_END} " +
                    $"=> Get your group ID {MessageFormatSignal.NEWLINE}" + MessageFormatSignal.DIVIDER + MessageFormatSignal.NEWLINE +
                $"{MessageFormatSignal.BOLD_START}log add [Contains-LogCategory]{MessageFormatSignal.BOLD_END} " +
                    $"==> Register to get log which has category name " +
                    $"{MessageFormatSignal.BOLD_START}contains [Contains-LogCategory]{MessageFormatSignal.BOLD_END}. " +
                    $"Example: log add Alpha;NAP {MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}log remove [LogCategory]{MessageFormatSignal.BOLD_END}{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}log start{MessageFormatSignal.BOLD_END} " +
                    $"=> Start receiving logs{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}log stop [TimeSpan(Optional)]{MessageFormatSignal.BOLD_END} " +
                    $"=> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                    $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}log status{MessageFormatSignal.BOLD_END} " +
                    $"=> Get your current subscribing Log Categories and Receiving Logs status{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.DIVIDER}{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}gitlab addProject [GitlabProjectUrl]{MessageFormatSignal.BOLD_END} " +
                    $"=> Register to get notification of Gitlab's project{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}gitlab removeProject [GitlabProjectUrl]{MessageFormatSignal.BOLD_END} " +
                    $"=> Disable getting notification of Gitlab's project{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.DIVIDER}{MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}um start{MessageFormatSignal.BOLD_END} " +
                    $"=> Start getting notification when UM starts {MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}um stop{MessageFormatSignal.BOLD_END} " +
                    $"=> Stop getting UM information {MessageFormatSignal.NEWLINE}" +
                $"{MessageFormatSignal.BOLD_START}um addPage [PageUrl]{MessageFormatSignal.BOLD_END} " +
                    $"=> Add page to check show UM in UM Time. For example: um addPage [http://page1.com;http://page2.com]";

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