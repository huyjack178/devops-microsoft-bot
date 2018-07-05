namespace Fanex.Bot.Skynex.Models.Log
{
    using System;
    using System.Net;
    using Fanex.Bot.Skynex.Utilities.Log;

    public class Log
    {
        private const string NewLine = "\n\n";

        public long LogId { get; set; }

        public string FormattedMessage { get; set; }

        public string Message => FormatMessage();

        public string FullMessage => FormatMessage(isDetail: true);

        public int NumMessage { get; set; }

        public LogCategory Category { get; set; }

        public Machine Machine { get; set; }

        private string FormatMessage(bool isDetail = false)
        {
            if (isDetail)
            {
                return FormatAll(FormattedMessage);
            }

            var requestInfoIndex = FormattedMessage.IndexOf("REQUEST INFO", StringComparison.InvariantCultureIgnoreCase);
            var isNotNewLogType = requestInfoIndex < 0;

            if (isNotNewLogType)
            {
                return FormattedMessage.Length > 400 ?
                    FormatAll(FormattedMessage.Substring(0, 400)) :
                    FormatAll(FormattedMessage);
            }

            var message = string.Empty;
            message += LogFormatter.FormatRequestInfo(FormattedMessage, Category);
            message += LogFormatter.FormatBrowserInfo(FormattedMessage);
            message += LogFormatter.FormatServerAndDatabaseInfo(FormattedMessage, Machine);
            message += LogFormatter.FormatCustomInfo(FormattedMessage);
            message += LogFormatter.FormatSessionInfo(FormattedMessage);
            message += LogFormatter.FormatExceptionInfo(FormattedMessage);

            return FormatAll(message);
        }

        private string FormatAll(string message)
        {
            var returnMessage = message.Replace("\r", "\n").Replace("\t", string.Empty)
                      .Replace("Timestamp", "**Timestamp**")
                      .Replace("Message", "**Message**")
                      .Replace("REQUEST INFO", "**REQUEST INFO**")
                      .Replace("BROWSER INFO", "**BROWSER INFO**")
                      .Replace("SERVER INFO", "**SERVER INFO**")
                      .Replace("DATABASE INFO", "**DATABASE INFO**")
                      .Replace("EXCEPTION INFO", "**EXCEPTION INFO**")
                      .Replace("REQUEST HEADERS", "**REQUEST HEADERS**")
                      .Replace("SESSION INFO", "**SESSION INFO**");

            return $"**Category**: {Category.CategoryName}{NewLine}" +
                    $"{WebUtility.HtmlDecode(returnMessage)}{NewLine}" +
                    $"**#Log Id**: {LogId} " +
                    $"**Count**: {NumMessage}{NewLine}{NewLine}" +
                    $"===================================={NewLine}";
        }
    }
}