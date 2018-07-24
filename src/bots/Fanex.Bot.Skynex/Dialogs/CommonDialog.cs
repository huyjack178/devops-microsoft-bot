﻿namespace Fanex.Bot.Skynex.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.Skynex.Models;
    using Fanex.Bot.Skynex.Utilities.Bot;
    using Connector = Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;

    public interface ICommonDialog : IDialog
    {
        Task RegisterMessageInfo(Connector.IMessageActivity activity);

        Task RemoveConversationData(Connector.IMessageActivity activity);
    }

    public class CommonDialog : Dialog, ICommonDialog
    {
        public CommonDialog(
            BotDbContext dbContext,
            IConversation conversation) : base(dbContext, conversation)
        {
        }

        public override async Task HandleMessageAsync(Connector.IMessageActivity activity, string message)
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

        public virtual async Task RegisterMessageInfo(Connector.IMessageActivity activity)
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

        public virtual async Task RemoveConversationData(Connector.IMessageActivity activity)
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

        private static MessageInfo InitMessageInfo(Connector.IMessageActivity activity)
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
    }
}