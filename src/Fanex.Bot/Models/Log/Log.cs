using System;

namespace Fanex.Bot.Models
{
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

            if (isNotNewLogType && FormattedMessage.Length > 400)
            {
                return FormatAll(FormattedMessage.Substring(0, 400));
            }

            var message = string.Empty;
            message = FormatRequestInfo(message);
            message = FormatBrowserInfo(message);
            message = FormatServerAndDatabaseInfo(message);
            message = FormatExceptionInfo(message);

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
                    $"{returnMessage}{NewLine}" +
                    $"**#Log Id**: {LogId} " +
                    $"**Count**: {NumMessage}{NewLine}{NewLine}" +
                    $"===================================={NewLine}";
        }

        private string FormatExceptionInfo(string message)
        {
            var exceptionInfoIndex = FormattedMessage.IndexOf("EXCEPTION INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (exceptionInfoIndex > 0)
            {
                var detailsIndex = FormattedMessage.IndexOf(
                    "Details:", exceptionInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var exceptionInfo = string.Empty;

                exceptionInfo = detailsIndex > 0 ?
                    FormattedMessage.Substring(exceptionInfoIndex, detailsIndex - exceptionInfoIndex) :
                    FormattedMessage.Substring(exceptionInfoIndex);

                exceptionInfo = exceptionInfo.Replace(
                    "EXCEPTION INFO", string.Empty, StringComparison.InvariantCultureIgnoreCase);

                returnMessage = message + $"{NewLine}**Exception:** {NewLine}{exceptionInfo}";
            }

            return returnMessage;
        }

        private string FormatServerAndDatabaseInfo(string message)
        {
            var returnMessage = message + $"{NewLine}**Server:** {Machine.MachineName} ({Machine.MachineIP})";

            var databaseInfoIndex = FormattedMessage.IndexOf("DATABASE INFO", StringComparison.InvariantCultureIgnoreCase);

            if (databaseInfoIndex > 0)
            {
                var serverIndex = FormattedMessage.IndexOf("Server:", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var parameterIndex = FormattedMessage.IndexOf("Parameters:", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var databaseInfo = "No information";

                if (serverIndex > 0 && parameterIndex > 0)
                {
                    databaseInfo = FormattedMessage.Substring(serverIndex, parameterIndex - serverIndex);
                }

                returnMessage += $"{NewLine}**Database:**{NewLine}{databaseInfo}";
            }

            return returnMessage;
        }

        private string FormatBrowserInfo(string message)
        {
            var browserInfoIndex = FormattedMessage.IndexOf("BROWSER INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (browserInfoIndex > 0)
            {
                var browserIndex = FormattedMessage.IndexOf("Browser:", browserInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var platformIndex = FormattedMessage.IndexOf("Platform:", browserInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var browser = "No information";

                if (browserIndex > 0 && platformIndex > 0)
                {
                    browser = FormattedMessage.Substring(browserIndex, platformIndex - browserIndex).Replace("Browser:", string.Empty, StringComparison.InvariantCultureIgnoreCase);
                }

                var mobileDeviceModelIndex = FormattedMessage.IndexOf("MobileDeviceModel:", browserInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var serverInfoIndex = FormattedMessage.IndexOf("SERVER INFO", StringComparison.InvariantCultureIgnoreCase);
                var mobileDeviceModel = "MobileDeviceModel: No infomation";

                if (mobileDeviceModelIndex > 0 && serverInfoIndex > 0)
                {
                    mobileDeviceModel = FormattedMessage.Substring(mobileDeviceModelIndex, serverInfoIndex - mobileDeviceModelIndex);
                }

                returnMessage = message + $"{NewLine}**Browser:** {browser} {mobileDeviceModel}";
            }

            return returnMessage;
        }

        private string FormatRequestInfo(string message)
        {
            var requestInfoIndex = FormattedMessage.IndexOf("REQUEST INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (requestInfoIndex > 0)
            {
                var urlIndex = FormattedMessage.IndexOf("Url:", requestInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var urlReferrerIndex = FormattedMessage.IndexOf("UrlReferrer:", requestInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var requestUrl = "No information";

                if (urlIndex > 0 && urlReferrerIndex > 0)
                {
                    requestUrl = FormattedMessage.Substring(urlIndex, urlReferrerIndex - urlIndex).Replace("Url:", string.Empty, StringComparison.InvariantCultureIgnoreCase);
                }

                returnMessage = message + FormattedMessage.Remove(requestInfoIndex);
                returnMessage += $"{NewLine}**Request:** {requestUrl}";
            }

            return returnMessage;
        }
    }
}