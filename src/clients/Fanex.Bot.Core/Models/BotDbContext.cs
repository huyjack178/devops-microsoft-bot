namespace Fanex.Bot.Models
{
    using Fanex.Bot.Models.GitLab;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Models.Sentry;
    using Fanex.Bot.Models.UM;
    using Fanex.Bot.Models.Zabbix;
    using Microsoft.EntityFrameworkCore;

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
    }
}