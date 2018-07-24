namespace Fanex.Bot.Skynex.Utilities.Log
{
    using System;
    using System.Text;
    using Fanex.Bot.Skynex.Models.Log;

    public static class LogFormatter
    {
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
                returnMessage += $"{Constants.NewLine}**Request:** " +
                    $"{CheckAndHideAlphaDomain(requestUrl, categoryName)}";
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

                returnMessage = $"{Constants.NewLine}**Browser:** {browser} {mobileDeviceModel}";
            }

            return returnMessage;
        }

        public static string FormatServerAndDatabaseInfo(string rawMessage, string machineName, string machineIP)
        {
            var returnMessage = $"{Constants.NewLine}**Server:** {machineName} ({machineIP})";

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

                returnMessage += $"{Constants.NewLine}**Database:**{Constants.NewLine}{databaseInfo}";
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
                returnMessage = $"{Constants.NewLine}**Custom Info:** {Constants.NewLine}{customInfo}";
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

                returnMessage = $"{Constants.NewLine}**Exception:** {Constants.NewLine}{exceptionInfo}";
            }

            return returnMessage;
        }

        public static string CheckAndHideAlphaDomain(string request, string categoryName)
        {
            var hideDomainRequest = request;

            if (categoryName.ToLowerInvariant().Contains("alpha"))
            {
                var requestUri = new Uri(request);

                if (!requestUri.Host.ToLowerInvariant().Contains("staging"))
                {
                    hideDomainRequest = $"http://alpha.site{requestUri.AbsolutePath}";
                }
            }

            return hideDomainRequest;
        }

        public static string FormatSessionInfo(string rawMessage)
        {
            var sessionInfoIndex = rawMessage.IndexOf(
                    LogFilterInfo.SessionInfo,
                    StringComparison.InvariantCultureIgnoreCase);
            var returnMessage = new StringBuilder();

            if (sessionInfoIndex > 0)
            {
                returnMessage.Append($"{Constants.NewLine}**Session Info:**");

                foreach (var key in LogFilterInfo.SessionInfoKeys)
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

                        returnMessage.Append(Constants.NewLine + value);
                    }
                }
            }

            return returnMessage.ToString();
        }
    }
}