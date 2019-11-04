using System;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Database;
using Fanex.Bot.Core.Zabbix.Models;
using Fanex.Bot.Core.Zabbix.Services;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex._Shared.MessageSenders;
using Microsoft.Bot.Connector;
using Microsoft.EntityFrameworkCore;

namespace Fanex.Bot.Skynex.Zabbix
{
    public interface IZabbixDialog : IDialog
    {
        Task ScanService();
    }

    public class ZabbixDialog : BaseDialog, IZabbixDialog
    {
        private readonly IZabbixService zabbixService;
        private readonly IZabbixMessageBuilder messageBuilder;

        public ZabbixDialog(
            BotDbContext dbContext,
            IConversation conversation,
            IZabbixService zabbixService,
            IZabbixMessageBuilder messageBuilder) :
                base(dbContext, conversation)
        {
            this.zabbixService = zabbixService;
            this.messageBuilder = messageBuilder;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var command = message.Replace(MessageCommand.ZABBIX, string.Empty).Trim();

            if (command.StartsWith(MessageCommand.ZABBIX_ENABLE_SCAN_SERVICE))
            {
                await EnableScanService(activity);
                return;
            }

            if (command.StartsWith(MessageCommand.ZABBIX_DISABLE_SCAN_SERVICE))
            {
                await EnableScanService(activity, isEnable: false);
                return;
            }

            if (command.StartsWith(MessageCommand.ZABBIX_SCAN_SERVICE))
            {
                await ScanService();
            }
        }

        public async Task EnableScanService(IMessageActivity activity, bool isEnable = true)
        {
            var zabbixInfo = await GetZabbixInfo(activity);
            zabbixInfo.EnableScanService = isEnable;
            await SaveZabbixInfo(zabbixInfo);

            var enableText = isEnable ? "enabled" : "disabled";
            await Conversation.ReplyAsync(activity, $"Zabbix scan service after UM has been {enableText}!");
        }

        public async Task ScanService()
        {
            var services = await zabbixService.GetServices();
            var serviceGroups = services.GroupBy(service => service.Interfaces.FirstOrDefault().IP);

            foreach (var serviceGroup in serviceGroups)
            {
                var message = messageBuilder.BuildMessage(serviceGroup);

                await SendScanServiceMessage(message);
            }
        }

        private async Task SaveZabbixInfo(ZabbixInfo zabbixInfo)
        {
            bool existZabbixInfo = await DbContext.ZabbixInfo.AsNoTracking().AnyAsync(e =>
                   e.ConversationId == zabbixInfo.ConversationId);

#pragma warning disable S109 // Magic numbers should not be used
            zabbixInfo.ModifiedTime = DateTime.UtcNow.AddHours(7);
#pragma warning restore S109 // Magic numbers should not be used
            DbContext.Entry(zabbixInfo).State = existZabbixInfo ? EntityState.Modified : EntityState.Added;

            await DbContext.SaveChangesAsync();
        }

#pragma warning disable S3994 // URI Parameters should not be strings

        private async Task<ZabbixInfo> GetZabbixInfo(IMessageActivity activity)
        {
            var zabbixInfo = await GetExistingZabbixInfo(activity);

            if (zabbixInfo == null)
            {
                zabbixInfo = new ZabbixInfo
                {
                    ConversationId = activity.Conversation.Id,
#pragma warning disable S109 // Magic numbers should not be used
                    CreatedTime = DateTime.UtcNow.AddHours(7),
#pragma warning restore S109 // Magic numbers should not be used
                };
            }

            return zabbixInfo;
        }

        private Task<ZabbixInfo> GetExistingZabbixInfo(IMessageActivity activity)
            => DbContext.ZabbixInfo
                .AsNoTracking()
                .FirstOrDefaultAsync(info => info.ConversationId == activity.Conversation.Id);

        private async Task SendScanServiceMessage(string message)
        {
            var zabbixInfos = DbContext.ZabbixInfo.Where(info => info.IsActive && info.EnableScanService);

            foreach (var zabbixInfo in zabbixInfos)
            {
                await Conversation.SendAsync(zabbixInfo.ConversationId, message);
            }
        }
    }
}