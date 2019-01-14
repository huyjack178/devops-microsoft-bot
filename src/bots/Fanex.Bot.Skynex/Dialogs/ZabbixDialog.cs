﻿namespace Fanex.Bot.Skynex.Dialogs
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Zabbix;
    using Fanex.Bot.Services;
    using Fanex.Bot.Skynex.MessageHandlers.MessageBuilders;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;

    public interface IZabbixDialog : IDialog
    {
        Task ScanService();
    }

    public class ZabbixDialog : BaseDialog, IZabbixDialog
    {
        private readonly IZabbixService zabbixService;
        private readonly IUnderMaintenanceService umService;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IZabbixMessageBuilder messageBuilder;

        public ZabbixDialog(
            BotDbContext dbContext,
            IConversation conversation,
            IZabbixService zabbixService,
            IUnderMaintenanceService umService,
            IRecurringJobManager recurringJobManager,
            IZabbixMessageBuilder messageBuilder) :
                base(dbContext, conversation)
        {
            this.zabbixService = zabbixService;
            this.umService = umService;
            this.recurringJobManager = recurringJobManager;
            this.messageBuilder = messageBuilder;
        }

        public async Task HandleMessage(IMessageActivity activity, string message)
        {
            var command = message.Replace(MessageCommand.ZABBIX, string.Empty).Trim();

            if (command.StartsWith(MessageCommand.ZABBIX_START_SCAN_SERVICE))
            {
                await StartScanService(activity);
            }
        }

        public async Task StartScanService(IMessageActivity activity)
        {
            recurringJobManager.AddOrUpdate(
                HangfireJob.ZABBIX_SCAN_SERVICE, Job.FromExpression(() => ScanService()), Cron.MinuteInterval(15));

            var zabbixInfo = await GetZabbixInfo(activity);
            zabbixInfo.EnableScanService = true;
            await SaveZabbixInfo(zabbixInfo);

            await Conversation.ReplyAsync(activity, "Zabbix scan service after UM has been started!");
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

            zabbixInfo.ModifiedTime = DateTime.UtcNow.AddHours(7);
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
                    CreatedTime = DateTime.UtcNow.AddHours(7),
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