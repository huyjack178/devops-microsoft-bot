namespace Fanex.Bot.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MessageInfo
    {
        public string FromId { get; set; }

        public string FromName { get; set; }

        public string ToId { get; set; }

        public string ToName { get; set; }

        public string ServiceUrl { get; set; }

        public string ChannelId { get; set; }

        [Key]
        public string ConversationId { get; set; }

        [NotMapped]
        public string Text { get; set; }
    }
}