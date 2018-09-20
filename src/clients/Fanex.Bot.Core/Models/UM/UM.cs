namespace Fanex.Bot.Models.UM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class UM
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsUM { get; set; }

        public int ErrorCode { get; set; }
    }
}