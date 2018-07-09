namespace Fanex.Bot.Skynex.Models.UM
{
    using System.ComponentModel.DataAnnotations;

    public class UMPage
    {
#pragma warning disable S3996 // URI properties should not be strings

        [Key]
        public string SiteUrl { get; set; }

#pragma warning restore S3996 // URI properties should not be strings

        public string Name { get; set; }

        public bool IsActive { get; set; } = true;
    }
}