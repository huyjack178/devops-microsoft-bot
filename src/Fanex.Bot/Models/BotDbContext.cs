namespace Fanex.Bot.Models
{
    using Fanex.Bot.Models;
    using Microsoft.EntityFrameworkCore;

    public class BotDbContext : DbContext
    {
        public BotDbContext(DbContextOptions<BotDbContext> options)
        : base(options)
        {
        }

        public virtual DbSet<MessageInfo> MessageInfo { get; set; }
    }
}