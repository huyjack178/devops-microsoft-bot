namespace Fanex.Bot.Dialogs.Impl
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Utilitites.Bot;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;

    public class Dialog : IDialog
    {
        private readonly BotDbContext _dbContext;

        public Dialog(
            BotDbContext dbContext,
            IConversation conversation)
        {
            _dbContext = dbContext;
            Conversation = conversation;
        }

        public IConversation Conversation { get; }

        public static string GetCommandMessages()
        {
            return $"Skynex's available commands:{Constants.NewLine}" +
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
                    $"**group** ==> Get your group ID";
        }

        public async Task RegisterMessageInfo(IMessageActivity activity)
        {
            var messageInfo = await _dbContext.MessageInfo.FirstOrDefaultAsync(
                e => e.ConversationId == activity.Conversation.Id);

            if (messageInfo == null)
            {
                messageInfo = InitMessageInfo(activity);
                await SaveMessageInfoAsync(messageInfo);
                await Conversation.SendAdminAsync($"New client **{activity.Conversation.Id}** has been added");
            }
        }

        public async Task RemoveConversationData(IMessageActivity activity)
        {
            var messageInfo = await _dbContext.MessageInfo.SingleOrDefaultAsync(
                info => info.ConversationId == activity.Conversation.Id);

            if (messageInfo != null)
            {
                _dbContext.MessageInfo.Remove(messageInfo);
            }

            var logInfo = await _dbContext.LogInfo.SingleOrDefaultAsync(
                info => info.ConversationId == activity.Conversation.Id);

            if (logInfo != null)
            {
                _dbContext.LogInfo.Remove(logInfo);
            }

            var gitlabInfo = await _dbContext.GitLabInfo.SingleOrDefaultAsync(
               info => info.ConversationId == activity.Conversation.Id);

            if (gitlabInfo != null)
            {
                _dbContext.GitLabInfo.Remove(gitlabInfo);
            }

            await _dbContext.SaveChangesAsync();
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
            _dbContext.Entry(messageInfo).State = existMessageInfo ? EntityState.Modified : EntityState.Added;

            await _dbContext.SaveChangesAsync();
        }

        private async Task<bool> ExistMessageInfo(MessageInfo messageInfo)
            => await _dbContext.MessageInfo.AnyAsync(e => e.ConversationId == messageInfo.ConversationId);
    }
}