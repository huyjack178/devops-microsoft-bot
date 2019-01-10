﻿namespace Fanex.Bot.Skynex.MessageHandlers.MessageBuilders
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

        private static string FinalizeMessage(string message, Log log)
        {
            var returnMessage = message
                    .Replace("\r", string.Empty)
                    .Replace("\t", string.Empty)
                    .Replace("\n", MessageFormatSignal.NEWLINE)
                    .Replace(MessageFormatSignal.NEWLINE + MessageFormatSignal.NEWLINE, MessageFormatSignal.NEWLINE)
                    .Replace(MessageFormatSignal.NEWLINE + MessageFormatSignal.NEWLINE, MessageFormatSignal.NEWLINE)
                    .Replace(MessageFormatSignal.NEWLINE + " " + MessageFormatSignal.NEWLINE, MessageFormatSignal.NEWLINE)
                    .Replace("Timestamp", $"{MessageFormatSignal.BOLD_START}Timestamp{MessageFormatSignal.BOLD_END}")
                    .Replace("Message", $"{MessageFormatSignal.BOLD_START}Message{MessageFormatSignal.BOLD_END}");

            return $"{MessageFormatSignal.BOLD_START}Category{MessageFormatSignal.BOLD_END}: {log.CategoryName}{MessageFormatSignal.NEWLINE}" +
                    $"{WebUtility.HtmlDecode(returnMessage)}{MessageFormatSignal.NEWLINE}" +
                    $"{MessageFormatSignal.BOLD_START}#Log Id{MessageFormatSignal.BOLD_END}: {log.LogId} " +
                    $"{MessageFormatSignal.BOLD_START}Count{MessageFormatSignal.BOLD_END}: " +
                    $"{log.NumMessage}{MessageFormatSignal.DOUBLE_NEWLINE}{MessageFormatSignal.DIVIDER}";
        }

        private static string FormatRequestInfo(string rawMessage, string categoryName)
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
                    $"{MessageFormatSignal.NEWLINE}{MessageFormatSignal.BOLD_START}Request:{MessageFormatSignal.BOLD_END} " +
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

                returnMessage = $"{MessageFormatSignal.NEWLINE}{MessageFormatSignal.BOLD_START}Browser:{MessageFormatSignal.BOLD_END}" +
                    $" {browser} {mobileDeviceModel.Trim('\n', ' ')}";
            }

            return returnMessage;
        }

        private static string FormatServerAndDatabaseInfo(string rawMessage, string machineName, string machineIP)
        {
            var returnMessage = $"{MessageFormatSignal.NEWLINE}{MessageFormatSignal.BOLD_START}Server:{MessageFormatSignal.BOLD_END} {machineName} ({machineIP})";

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

                returnMessage += $"{MessageFormatSignal.NEWLINE}{MessageFormatSignal.BOLD_START}Database:{MessageFormatSignal.BOLD_END}" +
                    $"{MessageFormatSignal.NEWLINE}{databaseInfo}";
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
                    "No information";

                customInfo = customInfo.Replace("CUSTOM INFO", string.Empty);
                returnMessage = $"{MessageFormatSignal.NEWLINE}{MessageFormatSignal.BOLD_START}Custom Info:{MessageFormatSignal.BOLD_END}" +
                    $" {MessageFormatSignal.NEWLINE}{customInfo}";
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

                returnMessage = $"{MessageFormatSignal.NEWLINE}{MessageFormatSignal.BOLD_START}Exception:{MessageFormatSignal.BOLD_END}" +
                    $" {MessageFormatSignal.NEWLINE}{exceptionInfo.Trim('\n', ' ')}";
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
                returnMessage.Append($"{MessageFormatSignal.NEWLINE}{MessageFormatSignal.BOLD_START}Session Info:{MessageFormatSignal.BOLD_END}");

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

                        returnMessage.Append(MessageFormatSignal.NEWLINE + value);
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
}