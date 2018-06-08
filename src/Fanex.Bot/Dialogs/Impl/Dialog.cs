namespace Fanex.Bot.Dialogs.Impl
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class Dialog : IDialog
    {
        private readonly IConfiguration _configuration;
        private readonly BotDbContext _dbContext;

        public Dialog(
            IConfiguration configuration,
            BotDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

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
                    $"**group** ==> Get your group ID";
        }

        public async Task SendAsync(MessageInfo messageInfo)
        {
            ConnectorClient connector = CreateConnectorClient(new Uri(messageInfo.ServiceUrl));

            var message = CreateMessageActivity(messageInfo);

            try
            {
                if (!string.IsNullOrEmpty(messageInfo.Text))
                {
                    message.Text = messageInfo.Text;
                    await connector.Conversations.SendToConversationAsync((Activity)message);
                }
            }
            catch (Exception ex)
            {
                await SendAdminAsync(
                    $"Error happen in client {messageInfo?.ConversationId}\n\n" +
                    $"Exception: {ex.InnerException.Message}");
            }
        }

        public async Task SendAsync(Activity activity, string message, bool notifyAdmin = true)
        {
            var connector = CreateConnectorClient(new Uri(activity.ServiceUrl));
            var reply = activity.CreateReply(message);

            await connector.Conversations.ReplyToActivityAsync(reply);

            if (!_dbContext.MessageInfo.Any(e => e.ConversationId == activity.Conversation.Id))
            {
                await RegisterMessageInfo(activity);
            }

            if (notifyAdmin)
            {
                await SendAdminAsync($"**{activity.From.Name} ({activity.From.Id})** has just sent **{activity.Text}** to {activity.Recipient.Name}");
            }
        }

        public async Task SendAsync(string conversationId, string message)
        {
            var messageInfo = _dbContext.MessageInfo.FirstOrDefault(info => info.ConversationId == conversationId);

            if (messageInfo != null)
            {
                messageInfo.Text = message;
                await SendAsync(messageInfo);
            }
            else
            {
                await SendAdminAsync($"Error: **{conversationId}** not found");
            }
        }

        public async Task RegisterMessageInfo(Activity activity)
        {
            var messageInfo = GetMessageInfo(activity);

            var isExisted = await SaveMessageInfoAsync(messageInfo);

            if (!isExisted)
            {
                await SendAdminAsync($"New client **{activity.Conversation.Id}** has been added");
            }
        }

        protected async Task SendAdminAsync(string message)
        {
            var adminMessageInfos = _dbContext.MessageInfo.Where(messageInfo => messageInfo.IsAdmin);

            foreach (var adminMessageInfo in adminMessageInfos)
            {
                var connector = CreateConnectorClient(new Uri(adminMessageInfo.ServiceUrl));
                var adminMessage = CreateMessageActivity(adminMessageInfo);
                adminMessage.Text = message;

                await connector.Conversations.SendToConversationAsync((Activity)adminMessage);
            }
        }

        protected MessageInfo GetMessageInfo(Activity activity)
        {
            var messageInfo = _dbContext.MessageInfo.FirstOrDefault(e => e.ConversationId == activity.Conversation.Id);

            if (messageInfo == null)
            {
                messageInfo = new MessageInfo
                {
                    ToId = activity.From.Id,
                    ToName = activity.From.Name,
                    FromId = activity.Recipient.Id,
                    FromName = activity.Recipient.Name,
                    ServiceUrl = activity.ServiceUrl,
                    ChannelId = activity.ChannelId,
                    ConversationId = activity.Conversation.Id,
                };
            }

            return messageInfo;
        }

        protected async Task<bool> SaveMessageInfoAsync(MessageInfo messageInfo)
        {
            bool existMessageInfo = ExistMessageInfo(messageInfo);
            _dbContext.Entry(messageInfo).State = existMessageInfo ? EntityState.Modified : EntityState.Added;

            await _dbContext.SaveChangesAsync();

            return existMessageInfo;
        }

        private bool ExistMessageInfo(MessageInfo messageInfo)
            => _dbContext.MessageInfo.Any(e => e.ConversationId == messageInfo.ConversationId);

        protected ConnectorClient CreateConnectorClient(Uri serviceUrl)
            => new ConnectorClient(
                            serviceUrl,
                            _configuration.GetSection("MicrosoftAppId").Value,
                            _configuration.GetSection("MicrosoftAppPassword").Value);

        private static IMessageActivity CreateMessageActivity(MessageInfo messageInfo)
        {
            var userAccount = new ChannelAccount(messageInfo.ToId, messageInfo.ToName);
            var botAccount = new ChannelAccount(messageInfo.FromId, messageInfo.FromName);
            var message = Activity.CreateMessageActivity();
            message.ChannelId = messageInfo.ChannelId;
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(id: messageInfo.ConversationId);

            return message;
        }
    }
}