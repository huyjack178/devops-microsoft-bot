namespace Fanex.Bot.Skynex.Models.Log
{
    using System;
    using System.Net;
    using Fanex.Bot.Skynex.Utilities.Log;

    public class Log
    {
        public long LogId { get; set; }

        public string FormattedMessage { get; set; }

        public int NumMessage { get; set; }

        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public string MachineName { get; set; }

        public string MachineIP { get; set; }

        public string Message => FormatMessage();

        public string FullMessage => FormatMessage(isDetail: true);

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
            message += LogFormatter.FormatRequestInfo(FormattedMessage, CategoryName);
            message += LogFormatter.FormatBrowserInfo(FormattedMessage);
            message += LogFormatter.FormatServerAndDatabaseInfo(FormattedMessage, MachineName, MachineIP);
            message += LogFormatter.FormatCustomInfo(FormattedMessage);
            message += LogFormatter.FormatSessionInfo(FormattedMessage);
            message += LogFormatter.FormatExceptionInfo(FormattedMessage);

            return FormatAll(message);
        }

        private string FormatAll(string message)
        {
            var returnMessage = message
                    .Replace("\r", "\n")
                    .Replace("\n\n", "\n")
                    .Replace("\n \n", "\n")
                    .Replace("\t", string.Empty)
                    .Replace("Timestamp", "**Timestamp**")
                    .Replace("Message", "**Message**")
                    .Replace("REQUEST INFO", "**REQUEST INFO**")
                    .Replace("BROWSER INFO", "**BROWSER INFO**")
                    .Replace("SERVER INFO", "**SERVER INFO**")
                    .Replace("DATABASE INFO", "**DATABASE INFO**")
                    .Replace("EXCEPTION INFO", "**EXCEPTION INFO**")
                    .Replace("REQUEST HEADERS", "**REQUEST HEADERS**")
                    .Replace("SESSION INFO", "**SESSION INFO**");

            return $"**Category**: {CategoryName}{Constants.NewLine}" +
                    $"{WebUtility.HtmlDecode(returnMessage)}{Constants.NewLine}" +
                    $"**#Log Id**: {LogId} " +
                    $"**Count**: {NumMessage}{Constants.NewLine}{Constants.NewLine}" +
                    $"===================================={Constants.NewLine}";
        }
    }
}