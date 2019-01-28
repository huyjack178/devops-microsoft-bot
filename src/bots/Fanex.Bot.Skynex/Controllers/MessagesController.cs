namespace Fanex.Bot.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Bot;
    using Fanex.Bot.Enums;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;

    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly ICommonDialog commonDialog;
        private readonly ILogDialog logDialog;
        private readonly IGitLabDialog gitLabDialog;
        private readonly ILineDialog lineDialog;
        private readonly IUnderMaintenanceDialog umDialog;
        private readonly IConversation conversation;
        private readonly IConfiguration configuration;
        private readonly IDBLogDialog dbLogDialog;
        private readonly IZabbixDialog zabbixDialog;

#pragma warning disable S107 // Methods should not have too many parameters

        public MessagesController(
            ICommonDialog commonDialog,
            ILogDialog logDialog,
            IGitLabDialog gitLabDialog,
            ILineDialog lineDialog,
            IUnderMaintenanceDialog umDialog,
            IConversation conversation,
            IConfiguration configuration,
            IDBLogDialog dbLogDialog,
            IZabbixDialog zabbixDialog)
        {
            this.commonDialog = commonDialog;
            this.logDialog = logDialog;
            this.gitLabDialog = gitLabDialog;
            this.lineDialog = lineDialog;
            this.umDialog = umDialog;
            this.conversation = conversation;
            this.configuration = configuration;
            this.dbLogDialog = dbLogDialog;
            this.zabbixDialog = zabbixDialog;
        }

#pragma warning restore S107 // Methods should not have too many parameters

        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    await HandleMessage(await HandleForChannels(activity));
                    break;

                case ActivityTypes.ConversationUpdate:
                    await HandleConversationUpdate(activity);
                    break;

                case ActivityTypes.ContactRelationUpdate:
                    await HandleContactRelationUpdate(activity);
                    break;

                default:
                    return Ok();
            }

            return Ok();
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        [Route("Forward")]
        public async Task<OkObjectResult> Forward(string message, string conversationId, MessageType messageType = MessageType.Markdown)
        {
            var result = await conversation.SendAsync(conversationId, message, messageType);

            return Ok(result);
        }

        [HttpPost]
        [Route("ForwardWithToken")]
        public async Task<IActionResult> ForwardWithToken(string token, string message, string conversationId, MessageType messageType = MessageType.Markdown)
        {
            var validToken = configuration.GetSection("BotSecretToken")?.Value;

            if (validToken == token)
            {
                var result = await conversation.SendAsync(conversationId, message, messageType);
                return Ok(result);
            }

            return Unauthorized();
        }

        private async Task HandleMessage(IMessageActivity activity)
        {
            var botName = configuration.GetSection("BotName")?.Value;
            var message = BotHelper.GenerateMessage(activity.Text, botName);

            if (message.StartsWith("log"))
            {
                await logDialog.HandleMessage(activity, message);
            }
            else if (message.StartsWith("gitlab"))
            {
                await gitLabDialog.HandleMessage(activity, message);
            }
            else if (message.StartsWith(MessageCommand.UM))
            {
                await umDialog.HandleMessage(activity, message);
            }
            else if (message.StartsWith("dblog"))
            {
                await dbLogDialog.HandleMessage(activity, message);
            }
            else if (message.StartsWith(MessageCommand.ZABBIX))
            {
                await zabbixDialog.HandleMessage(activity, message);
            }
            else
            {
                await commonDialog.HandleMessage(activity, message);
            }
        }

        private async Task HandleConversationUpdate(Activity activity)
        {
            var conversationUpdate = activity.AsConversationUpdateActivity();
            var botId = configuration.GetSection("BotId")?.Value;

            if (conversationUpdate.MembersRemoved != null &&
                conversationUpdate.MembersRemoved.Any(mem => mem.Id == botId))
            {
                await commonDialog.RemoveConversationData(activity);
                return;
            }

            if (conversationUpdate.MembersAdded != null &&
                conversationUpdate.MembersAdded.Any(mem => mem.Id == botId))
            {
                await commonDialog.RegisterMessageInfo(activity);
            }

            await conversation.ReplyAsync(activity, "Hello. I am SkyNex.");
        }

        private async Task HandleContactRelationUpdate(Activity activity)
        {
            if (activity.Action?.ToLowerInvariant() == "remove")
            {
                await commonDialog.RemoveConversationData(activity);
            }
            else
            {
                await commonDialog.RegisterMessageInfo(activity);
                await conversation.ReplyAsync(activity, "Hello. I am SkyNex.");
            }
        }

        private async Task<Activity> HandleForChannels(Activity activity)
        {
            if (activity.From?.Name?.ToLowerInvariant() == Channel.LINE)
            {
                activity.Conversation.Id = activity.From.Id;
                activity.ChannelId = Channel.LINE;
                await lineDialog.RegisterMessageInfo(activity);
            }

            return activity;
        }
    }
}