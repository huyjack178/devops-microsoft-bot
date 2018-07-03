namespace Fanex.Bot.Skynex.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Models;
    using Fanex.Bot.Skynex.Utilities.Bot;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;

    public interface ILineDialog : IDialog
    {
    }

    public class LineDialog : Dialog, ILineDialog
    {
        private readonly BotDbContext _dbContext;

        public LineDialog(
         BotDbContext dbContext,
         IConversation conversation) : base(dbContext, conversation)
        {
            _dbContext = dbContext;
        }

        public override async Task RegisterMessageInfo(IMessageActivity activity)
        {
            var messageInfo = await _dbContext.MessageInfo.FirstOrDefaultAsync(
                 e => e.ConversationId == activity.From.Id);

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
                ChannelId = "line",
                ConversationId = activity.From.Id,
                CreatedTime = DateTime.UtcNow.AddHours(7)
            };
        }
    }
}