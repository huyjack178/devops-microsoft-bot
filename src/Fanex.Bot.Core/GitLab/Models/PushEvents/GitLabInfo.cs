using System.ComponentModel.DataAnnotations;
using Fanex.Bot.Core._Shared.Database;

#pragma warning disable S3996 // URI properties should not be strings

namespace Fanex.Bot.Core.GitLab.Models
{
    public class GitLabInfo : EntityBase
    {
        [Key]
        public string ProjectUrl { get; set; }

        public bool EnablePush { get; set; } = true;

        public bool EnablePipeline { get; set; } = false;
    }
}

#pragma warning restore S3996 // URI properties should not be strings