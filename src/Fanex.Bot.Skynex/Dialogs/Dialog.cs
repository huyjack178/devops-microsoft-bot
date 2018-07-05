namespace Fanex.Bot.Skynex.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Models;
    using Fanex.Bot.Skynex.Utilities.Bot;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;

    public interface IDialog
    {
        IConversation Conversation { get; }

        Task HandleMessageAsync(IMessageActivity activity, string message);

        Task RegisterMessageInfo(IMessageActivity activity);

        Task RemoveConversationData(IMessageActivity activity);
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

        public static string CommandMessages { get; protected set; }
            = $"Skynex's available commands:{Constants.NewLine}";

        public static string GetCommandMessages()
        {
            return $"Skynex's available commands:{Constants.NewLine}" +
                    $"**group** ==> Get your group ID" +
                    $"**log add [Contains-LogCategory]** " +
                        $"==> Register to get log which has category name **contains [Contains-LogCategory]**. " +
                        $"Example: log add Alpha;NAP {Constants.NewLine}" +
                    $"**log remove [LogCategory]**{Constants.NewLine}" +
                    $"**log start** ==> Start receiving logs{Constants.NewLine}" +
                    $"**log stop [TimeSpan(Optional)]** ==> Stop receiving logs for [TimeSpan] - Default is 10 minutes. " +
                        $"TimeSpan format is *d*(day), *h*(hour), *m*(minute), *s*(second){Constants.NewLine}" +
                    $"**log detail [LogId] (BETA)** ==> Get log detail{Constants.NewLine}" +
                    $"**log status** ==> Get your current subscribing Log Categories and Receiving Logs status{Constants.NewLine}" +
                    $"**gitlab addProject [GitlabProjectUrl]** => Register to get notification of Gitlab's project{Constants.NewLine}" +
                    $"**gitlab removeProject [GitlabProjectUrl]** => Disable getting notification of Gitlab's project{Constants.NewLine}" +
                    $"**um start** ==> Start get notification when UM starts {Constants.NewLine}" +
                    $"**um addPage [PageUrl]** ==> Add page to check show UM in UM Time. For example: um addPage [http://page1.com;http://page2.com]";
        }

        public virtual async Task HandleMessageAsync(IMessageActivity activity, string message)
        {
            if (message.StartsWith("group"))
            {
                await Conversation.ReplyAsync(activity, $"Your group id is: {activity.Conversation.Id}");
                return;
            }

            if (message.StartsWith("help"))
            {
                await Conversation.ReplyAsync(activity, GetCommandMessages());
                return;
            }

            await Conversation.ReplyAsync(activity, "Please send **help** to get my commands");
        }

        public virtual async Task RegisterMessageInfo(IMessageActivity activity)
        {
            var messageInfo = await DbContext.MessageInfo.FirstOrDefaultAsync(
                e => e.ConversationId == activity.Conversation.Id);

            if (messageInfo == null)
            {
                messageInfo = InitMessageInfo(activity);
                await SaveMessageInfoAsync(messageInfo);
                await Conversation.SendAdminAsync($"New client **{activity.Conversation.Id}** has been added");
            }
        }

        public virtual async Task RemoveConversationData(IMessageActivity activity)
        {
            var messageInfo = await DbContext.MessageInfo.SingleOrDefaultAsync(
                info => info.ConversationId == activity.Conversation.Id);

            if (messageInfo != null)
            {
                DbContext.MessageInfo.Remove(messageInfo);
            }

            var logInfo = await DbContext.LogInfo.SingleOrDefaultAsync(
                info => info.ConversationId == activity.Conversation.Id);

            if (logInfo != null)
            {
                DbContext.LogInfo.Remove(logInfo);
            }

            var gitlabInfo = await DbContext.GitLabInfo.SingleOrDefaultAsync(
               info => info.ConversationId == activity.Conversation.Id);

            if (gitlabInfo != null)
            {
                DbContext.GitLabInfo.Remove(gitlabInfo);
            }

            await DbContext.SaveChangesAsync();
            await Conversation.SendAdminAsync($"Client **{activity.Conversation.Id}** has been removed");
        }

        private static MessageInfo InitMessageInfo(IMessageActivity activity)
        {
            return new MessageInfo
            {
                ToId = activity.From.Id,
                ToName = activity.From.Name,
                FromId = activity.Recipient.Id,
                FromName = activity.Recipient.Name,
                ServiceUrl = activity.ServiceUrl,
                ChannelId = activity.ChannelId,
                ConversationId = activity.Conversation.Id,
                CreatedTime = DateTime.UtcNow.AddHours(7)
            };
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