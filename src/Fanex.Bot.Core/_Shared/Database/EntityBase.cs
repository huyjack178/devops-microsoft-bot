using System;
using System.ComponentModel.DataAnnotations;

namespace Fanex.Bot.Core._Shared.Database
{
    public class EntityBase
    {
        [Key]
        public string ConversationId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedTime { get; set; }

        public DateTime ModifiedTime { get; set; }
    }
}