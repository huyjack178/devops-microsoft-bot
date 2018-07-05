#pragma warning disable S3996 // URI properties should not be strings

namespace Fanex.Bot.Skynex.Models.GitLab
{
    using System.ComponentModel.DataAnnotations;

    public class GitLabInfo : BaseInfo
    {
        [Key]
        public string ProjectUrl { get; set; }
    }
}

#pragma warning restore S3996 // URI properties should not be strings