namespace Fanex.Bot.Models.UM
{
    using System;

    public class UM
    {
        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public bool IsUnderMaintenanceTime { get; set; }

        public ConnectionResult ConnectionResult { get; set; }
    }

    public class ConnectionResult
    {
        public bool IsOk { get; set; }

        public bool IsNotOk { get; set; }

        public string Message { get; set; }
    }
}