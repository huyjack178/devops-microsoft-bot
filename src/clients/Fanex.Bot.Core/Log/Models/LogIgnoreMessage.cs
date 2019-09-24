using System.ComponentModel.DataAnnotations;

namespace Fanex.Bot.Core.Log.Models
{
    public class LogIgnoreMessage
    {
        [Key]
        public string Category { get; set; }

        [Key]
        public string IgnoreMessage { get; set; }
    }
}