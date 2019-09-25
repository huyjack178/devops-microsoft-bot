using System;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Utilities.Bot;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;

namespace Fanex.Bot.Skynex.Bot
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IConversation conversation;
        private readonly IConfiguration configuration;
        private readonly ICommonDialog commonDialog;
        private readonly Func<string, IDialog> functionFactory;

#pragma warning disable S107 // Methods should not have too many parameters

        public MessagesController(
            IConversation conversation,
            IConfiguration configuration,
            ICommonDialog commonDialog,
            Func<string, IDialog> functionFactory)
        {
            this.conversation = conversation;
            this.configuration = configuration;
            this.commonDialog = commonDialog;
            this.functionFactory = functionFactory;
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
                    await commonDialog.HandleConversationUpdate(activity);
                    break;

                case ActivityTypes.ContactRelationUpdate:
                    await commonDialog.HandleContactRelationUpdate(activity);
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
            var messageParts = message?.Split(" ");

            if (messageParts?.Length > 0)
            {
                var functionName = messageParts[0];
                var functionDialog = functionFactory(functionName);

                await functionDialog.HandleMessage(activity, message);
            }
        }
    }
}