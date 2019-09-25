using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.EntityFrameworkCore;
using Connector = Microsoft.Bot.Connector;

namespace Fanex.Bot.Skynex._Shared.Base
{
    public interface IDialog
    {
        Task HandleMessage(Connector.IMessageActivity activity, string message);
    }

    public class BaseDialog
    {
        protected BaseDialog(
            BotDbContext dbContext,
            IConversation conversation)
        {
            DbContext = dbContext;
            Conversation = conversation;
        }

        public IConversation Conversation { get; }

        public BotDbContext DbContext { get; }

        public static string GetCommandMessages()
            => $"Skynex's available commands:{MessageFormatSymbol.NEWLINE} " +
                $"{MessageFormatSymbol.BOLD_START}group{MessageFormatSymbol.BOLD_END} " +
                    $"=> Get your group ID {MessageFormatSymbol.NEWLINE}" + MessageFormatSymbol.DIVIDER + MessageFormatSymbol.NEWLINE +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} add [Contains-LogCategory]{MessageFormatSymbol.BOLD_END} " +
                    $"==> Register to get log which has category name " +
                    $"{MessageFormatSymbol.BOLD_START}contains [Contains-LogCategory]{MessageFormatSymbol.BOLD_END}. " +
                    $"Example: log add Alpha;NAP {MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} remove [LogCategory]{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} start{MessageFormatSymbol.BOLD_END} " +
                    $"=> Start receiving logs{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} stop [TimeSpan(Optional)]{MessageFormatSymbol.BOLD_END} " +
                    $"=> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                    $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogMSiteFunctionName} status{MessageFormatSymbol.BOLD_END} " +
                    $"=> Get your current subscribing Log Categories and Receiving Logs status{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.DIVIDER}{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}{FunctionType.LogDbFunctionName} start{MessageFormatSymbol.BOLD_END} " +
                $"=> Start to get log from Database (for DBA team){MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.DIVIDER}{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}gitlab addProject [GitlabProjectUrl]{MessageFormatSymbol.BOLD_END} " +
                    $"=> Register to get notification of Gitlab's project{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}gitlab removeProject [GitlabProjectUrl]{MessageFormatSymbol.BOLD_END} " +
                    $"=> Disable getting notification of Gitlab's project{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.DIVIDER}{MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}um start{MessageFormatSymbol.BOLD_END} " +
                    $"=> Start getting notification when UM starts {MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}um stop{MessageFormatSymbol.BOLD_END} " +
                    $"=> Stop getting UM information {MessageFormatSymbol.NEWLINE}" +
                $"{MessageFormatSymbol.BOLD_START}um addPage [PageUrl]{MessageFormatSymbol.BOLD_END} " +
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