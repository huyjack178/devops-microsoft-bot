using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core.AppCenter.Models;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.Bot.Connector;
using Microsoft.EntityFrameworkCore;

namespace Fanex.Bot.Skynex.AppCenter
{
    public interface IAppCenterDialog : IDialog
    {
        Task HandlePushEventAsync(AppCenterEvent pushEvent);
    }

#pragma warning disable S109 // Magic numbers should not be used

    public class AppCenterDialog : BaseDialog, IAppCenterDialog
    {
        private readonly IAppCenterMessageBuilder messageBuilder;

        public AppCenterDialog(
          BotDbContext dbContext,
          IConversation conversation,
          IAppCenterMessageBuilder messageBuilder)
          : base(dbContext, conversation)
        {
            this.messageBuilder = messageBuilder;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var messageParts = message.Split(" ");

            if (messageParts.Length > 1)
            {
                if (messageParts[1] == "start")
                {
                    await EnableDisableLog(activity, messageParts, true);
                }
                else if (messageParts[1] == "stop")
                {
                    await EnableDisableLog(activity, messageParts, false);
                }
                else
                {
                    await Conversation.ReplyAsync(activity, GetCommandMessages());
                }
            }
            else
            {
                await Conversation.ReplyAsync(activity, GetCommandMessages());
            }
        }

        protected async Task EnableDisableLog(IMessageActivity activity, string[] messageParts, bool enabled)
        {
            var enabledMessage = enabled ? "Enabled" : "Disabled";

            if (messageParts.Length > 2)
            {
                var projectName = messageParts[2]?.ToLowerInvariant();
                var appCenterInfo = await GetOrCreateAppCenterInfo(activity, projectName);

                if (appCenterInfo != null)
                {
                    appCenterInfo.IsActive = enabled;
                    await SaveAppCenterInfo(appCenterInfo);

                    var message = $"App Center Log has been {enabledMessage} for project " +
                                  $"{MessageFormatSymbol.BOLD_START}{projectName}{MessageFormatSymbol.BOLD_END}!";
                    await Conversation.ReplyAsync(activity, message);
                }
            }
            else
            {
                var appCenterInfos = GetAllAppCenterInfos(activity);

                if (appCenterInfos == null)
                {
                    await Conversation.ReplyAsync(activity, "Not found your App Center notification info");
                    return;
                }

                foreach (var appCenterInfo in appCenterInfos)
                {
                    appCenterInfo.IsActive = enabled;
                    await SaveAppCenterInfo(appCenterInfo);
                }

                await Conversation.ReplyAsync(activity, $"App Center Log has been {enabledMessage}!");
            }
        }

        public async Task HandlePushEventAsync(AppCenterEvent pushEvent)
        {
            var message = messageBuilder.BuildMessage(pushEvent);
            var url = pushEvent.Url.ToLowerInvariant();

            foreach (var appCenterInfo in DbContext.AppCenterInfo.Where(info => url.Contains(info.Project)))
            {
                if (appCenterInfo?.IsActive == true)
                {
                    await Conversation.SendAsync(appCenterInfo.ConversationId, message);
                }
            }
        }

        private async Task<AppCenterInfo> GetOrCreateAppCenterInfo(IMessageActivity activity, string projectName)
            => await FindAppCenterInfo(activity, projectName)
                ?? new AppCenterInfo
                {
                    ConversationId = activity.Conversation.Id,
                    Project = projectName,
                    IsActive = true,
                    CreatedTime = DateTime.UtcNow
                };

        private IEnumerable<AppCenterInfo> GetAllAppCenterInfos(IMessageActivity activity)
            => DbContext.AppCenterInfo.Where(info
                   => info.ConversationId == activity.Conversation.Id);

        private async Task<AppCenterInfo> FindAppCenterInfo(IMessageActivity activity, string projectName)
            => await DbContext.AppCenterInfo.FirstOrDefaultAsync(info
                   => info.ConversationId == activity.Conversation.Id
                      && string.Equals(info.Project, projectName, StringComparison.InvariantCultureIgnoreCase));

        private async Task SaveAppCenterInfo(AppCenterInfo appCenterInfo)
        {
            var existInfo = DbContext
                    .AppCenterInfo
                    .AsNoTracking()
                    .Any(e => e.ConversationId == appCenterInfo.ConversationId && e.Project == appCenterInfo.Project);

            DbContext.Entry(appCenterInfo).State = existInfo ? EntityState.Modified : EntityState.Added;

            await DbContext.SaveChangesAsync();
        }
    }

#pragma warning restore S109 // Magic numbers should not be used
}