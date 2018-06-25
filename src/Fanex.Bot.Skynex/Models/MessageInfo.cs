namespace Fanex.Bot.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MessageInfo
    {
        [Key]
        public string ConversationId { get; set; }

        public string FromId { get; set; }

        public string FromName { get; set; }

        public string ToId { get; set; }

        public string ToName { get; set; }

#pragma warning disable S3996 // URI properties should not be strings
        public string ServiceUrl { get; set; }
#pragma warning restore S3996 // URI properties should not be strings

        public string ChannelId { get; set; }

        public bool IsAdmin { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        [NotMapped]
        public string Text { get; set; }
    }
}