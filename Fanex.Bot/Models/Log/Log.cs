namespace Fanex.Bot.Models
{
    using System;
    using System.Web;

    public class Log
    {
        public long LogId { get; set; }

        public int EventId { get; set; }

        public string Severity { get; set; }

        public string Title { get; set; }

        public DateTime TimeStamp { get; set; }

        public string AppDomainName { get; set; }

        public int Priority { get; set; }

        public string ProcessId { get; set; }

        public string ProcessName { get; set; }

        public string Win32ThreadId { get; set; }

        public string ThreadName { get; set; }

        public string Message { get; set; }

        public string FormattedMessage { get; set; }

        public string MonitoringUser { get; set; }

        public bool IsMonitored { get; set; }

        public string FullMessage
            => FormattedMessage.Replace("\r", "\n").Replace("\t", string.Empty)
                    .Replace("Timestamp", "**Timestamp**")
                    .Replace("Message", "**Message**")
                    .Replace("REQUEST INFO", "**REQUEST INFO**")
                    .Replace("BROWSER INFO", "**BROWSER INFO**")
                    .Replace("SERVER INFO", "**SERVER INFO**")
                    .Replace("DATABASE INFO", "**DATABASE INFO**")
                    .Replace("EXCEPTION INFO", "**EXCEPTION INFO**");

        public string LogDate => TimeStamp.ToString("MMM dd,yyyy hh:mm:ss tt");

        public string GroupMessageIds { get; set; }

        public int NumMessage { get; set; }

        public LogCategory Category { get; set; }

        public Machine Machine { get; set; }
    }
}