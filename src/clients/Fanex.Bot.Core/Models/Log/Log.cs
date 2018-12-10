using System;

namespace Fanex.Bot.Models.Log
{
    public class Log
    {
        public long LogId { get; set; }

        public string FormattedMessage { get; set; }

        public int NumMessage { get; set; }

        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public string MachineName { get; set; }

        public string MachineIP { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}