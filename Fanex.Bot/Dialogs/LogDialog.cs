namespace Fanex.Bot.Dialogs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Services;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;

    public class LogDialog : ILogDialog
    {
        private readonly ILogService logService;
        private IConfiguration configuration;

        public LogDialog(ILogService logService, IConfiguration configuration)
        {
            this.logService = logService;
            this.configuration = configuration;
        }

        public async Task NotifyLogAsync(ITurnContext context)
        {
            var errorLogText = await logService.GetErrorLog(DateTime.UtcNow.AddHours(-24), DateTime.UtcNow);

            if (!string.IsNullOrEmpty(errorLogText))
            {
                await context.SendActivity(errorLogText);
            }
            else
            {
                await context.SendActivity("No error log now");
            }
        }

        public void NotifyLogPeriodically(ITurnContext context)
        {
            var appCredentials = new MicrosoftAppCredentials(
                configuration.GetSection("MicrosoftAppId").Value,
                configuration.GetSection("MicrosoftAppPassword").Value);

            var message = context.Activity;
            var messageInfo = new MessageInfo
            {
                ToId = message.From.Id,
                ToName = message.From.Name,
                FromId = message.Recipient.Id,
                FromName = message.Recipient.Name,
                ServiceUrl = message.ServiceUrl,
                ChannelId = message.ChannelId,
                ConversationId = message.Conversation.Id,
                AppCredentials = appCredentials
            };

            var timer = new Timer(new TimerCallback(TimerEvent), messageInfo, 1000, 30000);
        }

        private async void TimerEvent(object target)
        {
            var messageInfo = (MessageInfo)target;

            var errorLogText = await logService.GetErrorLog();
            messageInfo.Text = errorLogText;

            await ConversationStarter.Resume(messageInfo);
        }
    }
}