namespace Fanex.Bot.Dialogs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Services;
    using Hangfire;
    using Microsoft.Bot.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class LogDialog : ILogDialog
    {
        private readonly ILogService logService;
        private IConfiguration configuration;
        private readonly BotDbContext dbContext;

        public LogDialog(ILogService logService, IConfiguration configuration, BotDbContext dbContext)
        {
            this.logService = logService;
            this.configuration = configuration;
            this.dbContext = dbContext;
        }

        public async Task NotifyLogAsync(ITurnContext context)
        {
            var errorLogs = await logService.GetErrorLogs(DateTime.UtcNow.AddHours(-24), DateTime.UtcNow);
            RegisterConversation(context);

            if (errorLogs.Any())
            {
                await context.SendActivity(errorLogs.FirstOrDefault().Message);
            }
            else
            {
                await context.SendActivity("No error log now");
            }
        }

        private void RegisterConversation(ITurnContext context)
        {
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
            };
            if (dbContext.MessageInfo.Any(e => e.ConversationId == messageInfo.ConversationId))
            {
                dbContext.Entry(messageInfo).State = EntityState.Modified;
            }
            else
            {
                dbContext.Entry(messageInfo).State = EntityState.Added;
            }

            dbContext.SaveChanges();
        }

        public void NotifyLogPeriodically()
        {
            RecurringJob.AddOrUpdate(() => SendLogAsync(), Cron.Minutely);
        }

        public async Task SendLogAsync()
        {
            var messageInfos = dbContext.MessageInfo.Where(x => x.ChannelId.Contains("skype"));
            var errorLogs = await logService.GetErrorLogs();

            if (!errorLogs.Any())
            {
                return;
            }

            if (messageInfos == null && !messageInfos.Any())
            {
                return;
            }

            foreach (var messageInfo in messageInfos)
            {
                foreach (var errorLog in errorLogs)
                {
                    messageInfo.Text = errorLog.Message;
                    await ConversationStarter.Resume(messageInfo);
                }
            }
        }
    }
}