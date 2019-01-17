namespace Fanex.Bot.Models.UM
{
    using System.ComponentModel.DataAnnotations;

    public class UMSite
    {
        [Key]
        public string SiteId { get; set; }

        public string SiteName { get; set; }
    }
}