namespace Fanex.Bot.Skynex.Models.GitLab
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class GitLabInfo
    {
        [Key]
        public string ConversationId { get; set; }

#pragma warning disable S3996 // URI properties should not be strings

        [Key]
        public string ProjectUrl { get; set; }

#pragma warning restore S3996 // URI properties should not be strings

        public bool IsActive { get; set; } = true;

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }
    }
}