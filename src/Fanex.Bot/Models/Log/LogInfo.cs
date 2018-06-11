namespace Fanex.Bot.Models.Log
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class LogInfo
    {
        [Key]
        public string ConversationId { get; set; }

        public string LogCategories { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }
    }
}