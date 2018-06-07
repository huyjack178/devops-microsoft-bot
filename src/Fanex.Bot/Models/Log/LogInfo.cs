namespace Fanex.Bot.Models.Log
{
    using System.ComponentModel.DataAnnotations;

    public class LogInfo
    {
        [Key]
        public string ConversationId { get; set; }

        public string LogCategories { get; set; }

        public bool IsActive { get; set; }
    }
}