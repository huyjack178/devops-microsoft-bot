namespace Fanex.Bot.Skynex.Line
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Line.Messaging;
    using global::Line.Messaging.Webhooks;
    using Microsoft.Bot.Connector.DirectLine;

    public interface ILineBotApp
    {
        Task RunAsync(IEnumerable<WebhookEvent> events);
    }

    public class LineBotApp : WebhookApplication, ILineBotApp
    {
        private readonly LineMessagingClient _messagingClient;
        private readonly DirectLineClient _directLineClient;

        public LineBotApp(
            LineMessagingClient lineMessagingClient,
            DirectLineClient directLineClient)
        {
            _messagingClient = lineMessagingClient;
            _directLineClient = directLineClient;
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            if (ev.Message.Type == EventMessageType.Text)
            {
                await HandleTextAsync(ev.ReplyToken, ((TextEventMessage)ev.Message).Text, ev.Source.Id);
            }
        }

        public async Task HandleTextAsync(string replyToken, string userMessage, string userId)
        {
            var activity = new Activity
            {
                Type = "message",
                Text = userMessage,
                From = new ChannelAccount(userId, "line"),
                Recipient = new ChannelAccount("line", "line")
            };

            var conversation = await _directLineClient.Conversations.StartConversationAsync();

            await _directLineClient.Conversations.PostActivityAsync(conversation.ConversationId, activity);
        }

        public async Task GetAndReplyMessages(string replyToken, string userId, string conversationId)
        {
            var result = await _directLineClient.Conversations.GetActivitiesAsync(conversationId);

            var messages = GetMessages(result.Activities.LastOrDefault());

            await ReplyMessages(replyToken, userId, messages);
        }

        private static List<ISendMessage> GetMessages(Activity activity)
        {
            var messages = new List<ISendMessage>();

            if (!string.IsNullOrEmpty(activity.Text))
            {
                messages.Add(new TextMessage(activity.Text));
            }

            return messages;
        }

        private async Task ReplyMessages(string replyToken, string userId, List<ISendMessage> messages)
        {
            try
            {
                for (int i = 0; i < (double)messages.Count / 5; i++)
                {
                    if (i == 0)
                    {
                        await _messagingClient.ReplyMessageAsync(replyToken, messages.Take(5).ToList());
                    }
                    else
                    {
                        await _messagingClient.PushMessageAsync(replyToken, messages.Skip(i * 5).Take(5).ToList());
                    }
                }
            }
            catch
            {
                await _messagingClient.PushMessageAsync(userId, messages);
            }
        }
    }
}