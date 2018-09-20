namespace Fanex.Bot.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class BaseInfo
    {
        [Key]
        public string ConversationId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }
    }
}