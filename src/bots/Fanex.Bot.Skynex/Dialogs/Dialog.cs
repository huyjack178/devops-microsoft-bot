namespace Fanex.Bot.Skynex.Dialogs
{
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Models;
    using Fanex.Bot.Skynex.Utilities.Bot;
    using Microsoft.EntityFrameworkCore;
    using Connector = Microsoft.Bot.Connector;

    public interface IDialog
    {
        Task HandleMessageAsync(Connector.IMessageActivity activity, string message);
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
            return $"Skynex's available commands:{Constants.NewLine} " +
                    $"------------- {Constants.NewLine}" +
                    $"**group** ==> Get your group ID {Constants.NewLine}" +
                    $"------------- {Constants.NewLine}" +
                    $"**log add [Contains-LogCategory]** " +
                        $"==> Register to get log which has category name **contains [Contains-LogCategory]**. " +
                        $"Example: log add Alpha;NAP {Constants.NewLine}" +
                    $"**log remove [LogCategory]**{Constants.NewLine}" +
                    $"**log start** ==> Start receiving logs{Constants.NewLine}" +
                    $"**log stop [TimeSpan(Optional)]** ==> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                        $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){Constants.NewLine}" +
                    $"**log detail [LogId] (BETA)** ==> Get log detail{Constants.NewLine}" +
                    $"**log status** ==> Get your current subscribing Log Categories and Receiving Logs status{Constants.NewLine}" +
                    $"------------- {Constants.NewLine}" +
                    $"**gitlab addProject [GitlabProjectUrl]** => Register to get notification of Gitlab's project{Constants.NewLine}" +
                    $"**gitlab removeProject [GitlabProjectUrl]** => Disable getting notification of Gitlab's project{Constants.NewLine}" +
                    $"------------- {Constants.NewLine}" +
                    $"**um start** ==> Start get notification when UM starts {Constants.NewLine}" +
                    $"**um addPage [PageUrl]** ==> Add page to check show UM in UM Time. For example: um addPage [http://page1.com;http://page2.com]";
        }

        public virtual Task HandleMessageAsync(Connector.IMessageActivity activity, string message)
        {
            throw new System.NotImplementedException();
        }

        protected async Task SaveMessageInfoAsync(MessageInfo messageInfo)
        {
            var existMessageInfo = await ExistMessageInfo(messageInfo);
            DbContext.Entry(messageInfo).State = existMessageInfo ? EntityState.Modified : EntityState.Added;

            await DbContext.SaveChangesAsync();
        }

        private async Task<bool> ExistMessageInfo(MessageInfo messageInfo)
            => await DbContext.MessageInfo.AnyAsync(e => e.ConversationId == messageInfo.ConversationId);
    }
}