using System.ComponentModel.DataAnnotations;

namespace Fanex.Bot.Core.UM.Models
{
    public class UMSite
    {
        [Key]
        public string SiteId { get; set; }

        public string SiteName { get; set; }
    }
}