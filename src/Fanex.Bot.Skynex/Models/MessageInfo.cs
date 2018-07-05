﻿namespace Fanex.Bot.Skynex.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class MessageInfo : BaseInfo
    {
        public string FromId { get; set; }

        public string FromName { get; set; }

        public string ToId { get; set; }

        public string ToName { get; set; }

#pragma warning disable S3996 // URI properties should not be strings
        public string ServiceUrl { get; set; }
#pragma warning restore S3996 // URI properties should not be strings

        public string ChannelId { get; set; }

        public bool IsAdmin { get; set; }

        [NotMapped]
        public string Text { get; set; }
    }
}