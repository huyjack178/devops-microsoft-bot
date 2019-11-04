using System;
using System.Net;
using System.Text;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Helpers;
using Fanex.Bot.Skynex._Shared.Base;

namespace Fanex.Bot.Skynex.Log
{
    public interface IWebLogMessageBuilder : IMessageBuilder
    {
    }

#pragma warning disable S109 // Magic numbers should not be used

    public class WebLogMessageBuilder : IWebLogMessageBuilder
    {
        private const string SessionInfo = "SESSION INFO";
        private const string NoInfo = "No information";

        private static readonly string[] SessionInfoKeys = new[] {
            "Username", "AccUserName", "CustRoleId", "custid","CustUname",
            "MemberID", "MemberUserName", "AgentID", "AgentUserName", "MasterID", "MasterUserName",
            "SuperID", "SusperUserName", "IsSyncCSCurrentCust", "IsInternal", "sitename"
        };

        public string BuildMessage(object model)
        {
            var log = DataHelper.Parse<Core.Log.Models.Log>(model);

            var requestInfoIndex = log.FormattedMessage.IndexOf("REQUEST INFO", StringComparison.InvariantCultureIgnoreCase);
            var isNotNewLogType = requestInfoIndex < 0;

            if (isNotNewLogType)
            {
                log.FormattedMessage = FormatRequestOfNormalMessage(log.FormattedMessage, log.CategoryName);

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

        private static string FinalizeMessage(string message, Core.Log.Models.Log log)
        {
            var returnMessage = message
                    .Replace("\r", string.Empty)
                    .Replace("\t", string.Empty)
                    .Replace("\n", MessageFormatSymbol.NEWLINE)
                    .Replace(MessageFormatSymbol.NEWLINE + MessageFormatSymbol.NEWLINE, MessageFormatSymbol.NEWLINE)
                    .Replace(MessageFormatSymbol.NEWLINE + MessageFormatSymbol.NEWLINE, MessageFormatSymbol.NEWLINE)
                    .Replace(MessageFormatSymbol.NEWLINE + " " + MessageFormatSymbol.NEWLINE, MessageFormatSymbol.NEWLINE)
                    .Replace("Timestamp", $"{MessageFormatSymbol.BOLD_START}Timestamp{MessageFormatSymbol.BOLD_END}")
                    .Replace("Message", $"{MessageFormatSymbol.BOLD_START}Message{MessageFormatSymbol.BOLD_END}");

            return $"{MessageFormatSymbol.BOLD_START}Category{MessageFormatSymbol.BOLD_END}: {log.CategoryName}{MessageFormatSymbol.NEWLINE}" +
                    $"{WebUtility.HtmlDecode(returnMessage)}{MessageFormatSymbol.NEWLINE}" +
                    $"{MessageFormatSymbol.BOLD_START}#Log Id{MessageFormatSymbol.BOLD_END}: {log.LogId} " +
                    $"{MessageFormatSymbol.BOLD_START}Count{MessageFormatSymbol.BOLD_END}: " +
                    $"{log.NumMessage}{MessageFormatSymbol.DOUBLE_NEWLINE}{MessageFormatSymbol.DIVIDER}";
        }

        private static string FormatRequestInfo(string rawMessage, string categoryName)
        {
            var requestInfoIndex = rawMessage.IndexOf("REQUEST INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (requestInfoIndex > 0)
            {
                var urlIndex = rawMessage.IndexOf("Url:", requestInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var urlReferrerIndex = rawMessage.IndexOf("UrlReferrer:", requestInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var requestUrl = NoInfo;

                if (urlIndex > 0 && urlReferrerIndex > 0)
                {
                    requestUrl = rawMessage.Substring(urlIndex, urlReferrerIndex - urlIndex)
                        .Replace("Url:", string.Empty);
                }

                returnMessage = rawMessage.Remove(requestInfoIndex);
                returnMessage = returnMessage.Trim('\n', ' ');
                returnMessage +=
                    $"{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.BOLD_START}Request:{MessageFormatSymbol.BOLD_END} " +
                    CheckAndHideAlphaDomain(requestUrl, categoryName);
            }

            return returnMessage;
        }

        private static string FormatBrowserInfo(string rawMessage)
        {
            var browserInfoIndex = rawMessage.IndexOf("BROWSER INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (browserInfoIndex > 0)
            {
                var browserIndex = rawMessage.IndexOf("Browser:", browserInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var platformIndex = rawMessage.IndexOf("Platform:", browserInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var browser = NoInfo;

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

                returnMessage = $"{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.BOLD_START}Browser:{MessageFormatSymbol.BOLD_END}" +
                    $" {browser} {mobileDeviceModel.Trim('\n', ' ')}";
            }

            return returnMessage;
        }

        private static string FormatServerAndDatabaseInfo(string rawMessage, string machineName, string machineIP)
        {
            var returnMessage = $"{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.BOLD_START}Server:{MessageFormatSymbol.BOLD_END} {machineName} ({machineIP})";

            var databaseInfoIndex = rawMessage.IndexOf("DATABASE INFO", StringComparison.InvariantCultureIgnoreCase);

            if (databaseInfoIndex > 0)
            {
                var serverIndex = rawMessage.IndexOf("Server:", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var customInfo = rawMessage.IndexOf("CUSTOM INFO", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
#pragma warning disable S1192 // String literals should not be duplicated
                var exceptionInfo = rawMessage.IndexOf("EXCEPTION INFO", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
#pragma warning restore S1192 // String literals should not be duplicated
                var databaseInfo = NoInfo;

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

                returnMessage += $"{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.BOLD_START}Database:{MessageFormatSymbol.BOLD_END}" +
                    $"{MessageFormatSymbol.NEWLINE}{databaseInfo}";
            }

            return returnMessage;
        }

        private static string FormatCustomInfo(string rawMessage)
        {
            var customInfoIndex = rawMessage.IndexOf("CUSTOM INFO", StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = string.Empty;

            if (customInfoIndex > 0)
            {
                var exceptionInfoIndex = rawMessage.IndexOf(
                    "EXCEPTION INFO", StringComparison.InvariantCultureIgnoreCase);
                var customInfo = exceptionInfoIndex > 0 ?
                    rawMessage.Substring(customInfoIndex, exceptionInfoIndex - customInfoIndex) :
                    NoInfo;

                customInfo = customInfo.Replace("CUSTOM INFO", string.Empty);
                returnMessage = $"{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.BOLD_START}Custom Info:{MessageFormatSymbol.BOLD_END}" +
                    $" {MessageFormatSymbol.NEWLINE}{customInfo}";
            }

            return returnMessage;
        }

        private static string FormatExceptionInfo(string rawMessage)
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

                returnMessage = $"{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.BOLD_START}Exception:{MessageFormatSymbol.BOLD_END}" +
                    $" {MessageFormatSymbol.NEWLINE}{exceptionInfo.Trim('\n', ' ')}";
            }

            return returnMessage;
        }

        private static string FormatSessionInfo(string rawMessage)
        {
            var sessionInfoIndex = rawMessage.IndexOf(
                    SessionInfo,
                    StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = new StringBuilder();

            if (sessionInfoIndex > 0)
            {
                returnMessage.Append($"{MessageFormatSymbol.NEWLINE}{MessageFormatSymbol.BOLD_START}Session Info:{MessageFormatSymbol.BOLD_END}");

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

                        returnMessage.Append(MessageFormatSymbol.NEWLINE + value);
                    }
                }
            }

            return returnMessage.ToString();
        }

        private static string FormatRequestOfNormalMessage(string rawMessage, string categoryName)
        {
            var formatedMessage = rawMessage;
            const string urlTag = "URL:";
            var urlIndex = rawMessage.IndexOf(urlTag, StringComparison.InvariantCultureIgnoreCase);

            if (urlIndex > 0)
            {
                var endLineIndex = rawMessage.IndexOf("\n", urlIndex, StringComparison.InvariantCultureIgnoreCase);
                var requestUrl = rawMessage
                    .Substring(urlIndex, endLineIndex - urlIndex)
                    .Replace(urlTag, string.Empty);

                var hideDomainRequestUrl = CheckAndHideAlphaDomain(requestUrl, categoryName);
                formatedMessage = formatedMessage.Replace(requestUrl, hideDomainRequestUrl);
            }

            const string referrerTag = "REFERRER:";
            var referrerIndex = rawMessage.IndexOf(referrerTag, StringComparison.InvariantCultureIgnoreCase);

            if (referrerIndex > 0)
            {
                var endLineIndex = rawMessage.IndexOf("\n", referrerIndex, StringComparison.InvariantCultureIgnoreCase);
                var requestUrl = rawMessage
                    .Substring(referrerIndex, endLineIndex - referrerIndex)
                    .Replace(referrerTag, string.Empty);

                var hideDomainRequestUrl = CheckAndHideAlphaDomain(requestUrl, categoryName);
                formatedMessage = formatedMessage.Replace(requestUrl, hideDomainRequestUrl);
            }

            return formatedMessage;
        }

        private static string CheckAndHideAlphaDomain(string request, string categoryName)
        {
            var hideDomainRequest = request;

            if (categoryName.IndexOf("alpha", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                var requestUri = new Uri(request);

                if (requestUri.Host.IndexOf("staging", StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    hideDomainRequest = request.Replace(requestUri.Host, "alpha.site");
                }
            }

            return hideDomainRequest;
        }
    }

#pragma warning restore S109 // Magic numbers should not be used
}