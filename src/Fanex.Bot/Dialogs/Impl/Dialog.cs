namespace Fanex.Bot.Dialogs.Impl
{
    using System;
    using System.Linq;
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
                    $"**log stop** ==> Stop receiving logs{Constants.NewLine}" +
                    $"**log detail [LogId] (BETA)** ==> Get log detail{Constants.NewLine}" +
                    $"**log viewStatus** ==> Get your current subscribing Log Categories and Receiving Logs status{Constants.NewLine}" +
                    $"**gitlab addProject [GitlabProjectUrl]** => Register to get notification of Gitlab's project{Constants.NewLine}" +
                    $"**gitlab removeProject [GitlabProjectUrl]** => Disable getting notification of Gitlab's project{Constants.NewLine}" +
                    $"**group** ==> Get your group ID";
        }

        public async Task RegisterMessageInfo(IMessageActivity activity)
        {
            var messageInfo = _dbContext.MessageInfo.FirstOrDefault(e => e.ConversationId == activity.Conversation.Id);

            if (messageInfo == null)
            {
                messageInfo = InitMessageInfo(activity);
                await SaveMessageInfoAsync(messageInfo);
                await Conversation.SendAdminAsync($"New client **{activity.Conversation.Id}** has been added");
            }
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
            var existMessageInfo = ExistMessageInfo(messageInfo);
            _dbContext.Entry(messageInfo).State = existMessageInfo ? EntityState.Modified : EntityState.Added;

            await _dbContext.SaveChangesAsync();
        }

        private bool ExistMessageInfo(MessageInfo messageInfo)
            => _dbContext.MessageInfo.Any(e => e.ConversationId == messageInfo.ConversationId);
    }
}