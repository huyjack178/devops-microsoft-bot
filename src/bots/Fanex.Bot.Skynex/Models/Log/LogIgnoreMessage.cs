namespace Fanex.Bot.Skynex.Models.Log
{
    using System.ComponentModel.DataAnnotations;

    public class LogIgnoreMessage
    {
        [Key]
        public string Category { get; set; }

        [Key]
        public string IgnoreMessage { get; set; }
    }
}