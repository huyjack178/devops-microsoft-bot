﻿namespace Fanex.Bot.Skynex.Models
{
    using Fanex.Bot.Skynex.Models.GitLab;
    using Fanex.Bot.Skynex.Models.Log;
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
        }

        public virtual DbSet<MessageInfo> MessageInfo { get; set; }

        public virtual DbSet<LogInfo> LogInfo { get; set; }

        public virtual DbSet<LogIgnoreMessage> LogIgnoreMessage { get; set; }

        public virtual DbSet<GitLabInfo> GitLabInfo { get; set; }
    }
}