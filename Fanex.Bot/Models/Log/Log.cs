﻿using System;

namespace Fanex.Bot.Models
{
    public class Log
    {
        public long LogId { get; set; }

        public bool IsMonitored { get; set; }

        public DateTime TimeStamp { get; set; }

        public string FormattedMessage { get; set; }

        public string Message => AnalyzeMessage();

        public string GroupMessageIds { get; set; }

        public int NumMessage { get; set; }

        public LogCategory Category { get; set; }

        public Machine Machine { get; set; }

        private string AnalyzeMessage()
        {
            var message = FormattedMessage.Replace("\r", "\n").Replace("\t", string.Empty)
                        .Replace("Timestamp", "**Timestamp**")
                        .Replace("Message", "**Message**")
                        .Replace("REQUEST INFO", "**REQUEST INFO**");
            var browserInfoIndex = message.IndexOf("browser info", StringComparison.InvariantCultureIgnoreCase);

            if (browserInfoIndex > 0)
            {
                message = message.Remove(browserInfoIndex);
            }
            else if (message.Length > 500)
            {
                message = message.Substring(0, 500);
            }

            return $"**Log Id**: {LogId}\n\n" +
                $"**Number of logs**: {NumMessage}\n\n" +
                $"**Category**: {Category.CategoryName}\n\n" +
                $"{message}\n\n" +
                $"\n\n------------------------------------------------------\n\n"; ;
        }
    }
}