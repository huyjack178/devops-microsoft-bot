using Fanex.Bot.Core.AppCenter.Models;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Core.GitLab.Models;
using Fanex.Bot.Core.Log.Models;
using Fanex.Bot.Core.Sentry.Models;
using Fanex.Bot.Core.UM.Models;
using Fanex.Bot.Core.Zabbix.Models;
using Microsoft.EntityFrameworkCore;

namespace Fanex.Bot.Core._Shared.Database
{
    public class BotDbContext : DbContext
    {
        public BotDbContext()
        {
        }

        public BotDbContext(DbContextOptions<BotDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GitLabInfo>()
                .HasKey(c => new { c.ConversationId, c.ProjectUrl });

            modelBuilder.Entity<LogIgnoreMessage>()
                .HasKey(c => new { c.Category, c.IgnoreMessage });

            modelBuilder.Entity<SentryInfo>()
               .HasKey(c => new { c.ConversationId, c.Project, c.Level });

            modelBuilder.Entity<AppCenterInfo>()
                .HasKey(c => new { c.ConversationId, c.Project });
        }

        public virtual DbSet<MessageInfo> MessageInfo { get; set; }

        public virtual DbSet<LogInfo> LogInfo { get; set; }

        public virtual DbSet<LogIgnoreMessage> LogIgnoreMessage { get; set; }

        public virtual DbSet<GitLabInfo> GitLabInfo { get; set; }

        public virtual DbSet<UMInfo> UMInfo { get; set; }

        public virtual DbSet<UMPage> UMPage { get; set; }

        public virtual DbSet<UMSite> UMSite { get; set; }

        public virtual DbSet<SentryInfo> SentryInfo { get; set; }

        public virtual DbSet<ZabbixInfo> ZabbixInfo { get; set; }

        public virtual DbSet<AppCenterInfo> AppCenterInfo { get; set; }
    }
}