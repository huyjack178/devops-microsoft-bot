namespace Fanex.Bot.Skynex.MessageHandlers.MessageBuilders
{
    using System;
    using System.Net;
    using System.Text;
    using Fanex.Bot.Helpers;
    using Fanex.Bot.Models.Log;

    public interface IWebLogMessageBuilder : IMessageBuilder
    {
    }

    public class WebLogMessageBuilder : IWebLogMessageBuilder
    {
        private const string SessionInfo = "SESSION INFO";

        private static readonly string[] SessionInfoKeys = new[] {
            "Username", "AccUserName", "CustRoleId", "custid","CustUname",
            "MemberID", "MemberUserName", "AgentID", "AgentUserName", "MasterID", "MasterUserName",
            "SuperID", "SusperUserName", "IsSyncCSCurrentCust", "IsInternal", "sitename"
        };

        public string BuildMessage(object model)
        {
            var log = DataHelper.Parse<Log>(model);

            var requestInfoIndex = log.FormattedMessage.IndexOf("REQUEST INFO", StringComparison.InvariantCultureIgnoreCase);
            var isNotNewLogType = requestInfoIndex < 0;

            if (isNotNewLogType)
            {
                return log.FormattedMessage.Length > 400 ?
                    FinalizeMessage(log.FormattedMessage.Substring(0, 400), log) :
                    FinalizeMessage(log.FormattedMessage, log);
            }

            var logMessage = string.Empty;
            logMessage += FormatRequestInfo(log.FormattedMessage, log.CategoryName);
            logMessage += FormatBrowserInfo(log.FormattedMessage);
            logMessage += FormatServerAndDatabaseInfo(log.FormattedMessage, log.MachineName, log.MachineIP);
            logMessage += FormatCustomInfo(log.FormattedMessage);
            logMessage += FormatSessionInfo(log.FormattedMessage);
            logMessage += FormatExceptionInfo(log.FormattedMessage);

            return FinalizeMessage(logMessage, log);
        }

        private static string FinalizeMessage(string message, Log log)
        {
            var returnMessage = message
                    .Replace("\r", string.Empty)
                    .Replace("\t", string.Empty)
                    .Replace("\n", MessageFormatSignal.NewLine)
                    .Replace(MessageFormatSignal.NewLine + MessageFormatSignal.NewLine, MessageFormatSignal.NewLine)
                    .Replace(MessageFormatSignal.NewLine + MessageFormatSignal.NewLine, MessageFormatSignal.NewLine)
                    .Replace(MessageFormatSignal.NewLine + " " + MessageFormatSignal.NewLine, MessageFormatSignal.NewLine)
                    .Replace("Timestamp", $"{MessageFormatSignal.BeginBold}Timestamp{MessageFormatSignal.EndBold}")
                    .Replace("Message", $"{MessageFormatSignal.BeginBold}Message{MessageFormatSignal.EndBold}");

            return $"{MessageFormatSignal.BeginBold}Category{MessageFormatSignal.EndBold}: {log.CategoryName}{MessageFormatSignal.NewLine}" +
                    $"{WebUtility.HtmlDecode(returnMessage)}{MessageFormatSignal.NewLine}" +
                    $"{MessageFormatSignal.BeginBold}#Log Id{MessageFormatSignal.EndBold}: {log.LogId} " +
                    $"{MessageFormatSignal.BeginBold}Count{MessageFormatSignal.EndBold}: " +
                    $"{log.NumMessage}{MessageFormatSignal.DoubleNewLine}{MessageFormatSignal.BreakLine}";
        }

        public static string FormatRequestInfo(string rawMessage, string categoryName)
        {
            var requestInfoIndex = rawMessage.IndexOf("REQUEST INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (requestInfoIndex > 0)
            {
                var urlIndex = rawMessage.IndexOf("Url:", requestInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var urlReferrerIndex = rawMessage.IndexOf("UrlReferrer:", requestInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var requestUrl = "No information";

                if (urlIndex > 0 && urlReferrerIndex > 0)
                {
                    requestUrl = rawMessage.Substring(urlIndex, urlReferrerIndex - urlIndex)
                        .Replace("Url:", string.Empty);
                }

                returnMessage = rawMessage.Remove(requestInfoIndex);
                returnMessage = returnMessage.Trim('\n', ' ');
                returnMessage +=
                    $"{MessageFormatSignal.NewLine}{MessageFormatSignal.BeginBold}Request:{MessageFormatSignal.EndBold} " +
                    CheckAndHideAlphaDomain(requestUrl, categoryName);
            }

            return returnMessage;
        }

        public static string FormatBrowserInfo(string rawMessage)
        {
            var browserInfoIndex = rawMessage.IndexOf("BROWSER INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (browserInfoIndex > 0)
            {
                var browserIndex = rawMessage.IndexOf("Browser:", browserInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var platformIndex = rawMessage.IndexOf("Platform:", browserInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var browser = "No information";

                if (browserIndex > 0 && platformIndex > 0)
                {
                    browser = rawMessage.Substring(browserIndex, platformIndex - browserIndex)
                        .Replace("Browser:", string.Empty);
                }

                var mobileDeviceModelIndex = rawMessage.IndexOf("MobileDeviceModel:", browserInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var serverInfoIndex = rawMessage.IndexOf("SERVER INFO", StringComparison.InvariantCultureIgnoreCase);
                var mobileDeviceModel = "MobileDeviceModel: No infomation";

                if (mobileDeviceModelIndex > 0 && serverInfoIndex > 0)
                {
                    mobileDeviceModel = rawMessage.Substring(mobileDeviceModelIndex, serverInfoIndex - mobileDeviceModelIndex);
                }

                returnMessage = $"{MessageFormatSignal.NewLine}{MessageFormatSignal.BeginBold}Browser:{MessageFormatSignal.EndBold}" +
                    $" {browser} {mobileDeviceModel.Trim('\n', ' ')}";
            }

            return returnMessage;
        }

        public static string FormatServerAndDatabaseInfo(string rawMessage, string machineName, string machineIP)
        {
            var returnMessage = $"{MessageFormatSignal.NewLine}{MessageFormatSignal.BeginBold}Server:{MessageFormatSignal.EndBold} {machineName} ({machineIP})";

            var databaseInfoIndex = rawMessage.IndexOf("DATABASE INFO", StringComparison.InvariantCultureIgnoreCase);

            if (databaseInfoIndex > 0)
            {
                var serverIndex = rawMessage.IndexOf("Server:", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var customInfo = rawMessage.IndexOf("CUSTOM INFO", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var exceptionInfo = rawMessage.IndexOf("EXCEPTION INFO", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var databaseInfo = "No information";

                if (serverIndex > 0 && customInfo > 0)
                {
                    databaseInfo = rawMessage.Substring(serverIndex, customInfo - serverIndex);
                }
                else
                {
                    if (serverIndex > 0 && exceptionInfo > 0)
                    {
                        databaseInfo = rawMessage.Substring(serverIndex, exceptionInfo - serverIndex);
                    }
                }

                returnMessage += $"{MessageFormatSignal.NewLine}{MessageFormatSignal.BeginBold}Database:{MessageFormatSignal.EndBold}" +
                    $"{MessageFormatSignal.NewLine}{databaseInfo}";
            }

            return returnMessage;
        }

        public static string FormatCustomInfo(string rawMessage)
        {
            var customInfoIndex = rawMessage.IndexOf("CUSTOM INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (customInfoIndex > 0)
            {
                var exceptionInfoIndex = rawMessage.IndexOf(
                    "EXCEPTION INFO", StringComparison.InvariantCultureIgnoreCase);
                var customInfo = exceptionInfoIndex > 0 ?
                    rawMessage.Substring(customInfoIndex, exceptionInfoIndex - customInfoIndex) :
                    "No information";

                customInfo = customInfo.Replace("CUSTOM INFO", string.Empty);
                returnMessage = $"{MessageFormatSignal.NewLine}{MessageFormatSignal.BeginBold}Custom Info:{MessageFormatSignal.EndBold}" +
                    $" {MessageFormatSignal.NewLine}{customInfo}";
            }

            return returnMessage;
        }

        public static string FormatExceptionInfo(string rawMessage)
        {
            var exceptionInfoIndex = rawMessage.IndexOf("EXCEPTION INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (exceptionInfoIndex > 0)
            {
                var detailsIndex = rawMessage.IndexOf(
                    "Details:", exceptionInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var exceptionInfo = string.Empty;

                exceptionInfo = detailsIndex > 0 ?
                    rawMessage.Substring(exceptionInfoIndex, detailsIndex - exceptionInfoIndex) :
                    rawMessage.Substring(exceptionInfoIndex);

                exceptionInfo = exceptionInfo.Replace(
                    "EXCEPTION INFO", string.Empty);

                returnMessage = $"{MessageFormatSignal.NewLine}{MessageFormatSignal.BeginBold}Exception:{MessageFormatSignal.EndBold}" +
                    $" {MessageFormatSignal.NewLine}{exceptionInfo.Trim('\n', ' ')}";
            }

            return returnMessage;
        }

        public static string CheckAndHideAlphaDomain(string request, string categoryName)
        {
            var hideDomainRequest = request;

            if (categoryName.IndexOf("alpha", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                var requestUri = new Uri(request);

                if (requestUri.Host.IndexOf("staging", StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    hideDomainRequest = $"http://alpha.site{requestUri.AbsolutePath}";
                }
            }

            return hideDomainRequest;
        }

        public static string FormatSessionInfo(string rawMessage)
        {
            var sessionInfoIndex = rawMessage.IndexOf(
                    SessionInfo,
                    StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = new StringBuilder();

            if (sessionInfoIndex > 0)
            {
                returnMessage.Append($"{MessageFormatSignal.NewLine}{MessageFormatSignal.BeginBold}Session Info:{MessageFormatSignal.EndBold}");

                foreach (var key in SessionInfoKeys)
                {
                    var keyIndex = rawMessage.IndexOf(
                        key, sessionInfoIndex,
                        StringComparison.InvariantCultureIgnoreCase);

                    if (keyIndex <= 0)
                    {
                        continue;
                    }

                    var firstNewLineCharIndex = rawMessage.IndexOf("\n", keyIndex, StringComparison.InvariantCultureIgnoreCase);

                    if (firstNewLineCharIndex > keyIndex)
                    {
                        var value = rawMessage.Substring(keyIndex, firstNewLineCharIndex - keyIndex);

                        returnMessage.Append(MessageFormatSignal.NewLine + value);
                    }
                }
            }

            return returnMessage.ToString();
        }
    }
}