using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Utilities.Bot;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Fanex.Bot.Skynex.GitLab;
using Fanex.Bot.Skynex.Log;
using Fanex.Bot.Skynex.Sentry;
using Fanex.Bot.Skynex.UM;
using Fanex.Bot.Skynex.Zabbix;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;

namespace Fanex.Bot.Skynex.Bot
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly ICommonDialog commonDialog;
        private readonly ILogDialog logDialog;
        private readonly IGitLabDialog gitLabDialog;
        private readonly IUnderMaintenanceDialog umDialog;
        private readonly IConversation conversation;
        private readonly IConfiguration configuration;
        private readonly IDBLogDialog dbLogDialog;
        private readonly IZabbixDialog zabbixDialog;
        private readonly ISentryDialog sentryDialog;

#pragma warning disable S107 // Methods should not have too many parameters

        public MessagesController(
            ICommonDialog commonDialog,
            ILogDialog logDialog,
            IGitLabDialog gitLabDialog,
            IUnderMaintenanceDialog umDialog,
            IConversation conversation,
            IConfiguration configuration,
            IDBLogDialog dbLogDialog,
            ISentryDialog sentryDialog,
            IZabbixDialog zabbixDialog)
        {
            this.commonDialog = commonDialog;
            this.logDialog = logDialog;
            this.gitLabDialog = gitLabDialog;
            this.umDialog = umDialog;
            this.conversation = conversation;
            this.configuration = configuration;
            this.dbLogDialog = dbLogDialog;
            this.zabbixDialog = zabbixDialog;
            this.sentryDialog = sentryDialog;
        }

#pragma warning restore S107 // Methods should not have too many parameters

        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    await HandleMessage(activity);
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
            else if (message.StartsWith(MessageCommand.SENTRY_LOG))
            {
                await sentryDialog.HandleMessage(activity, message);
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
    }
}