﻿namespace Fanex.Bot.Utilities.Log
{
    using System;
    using Fanex.Bot.Models.Log;

    public static class LogFormatter
    {
        public static string FormatRequestInfo(string rawMessage, LogCategory category)
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
                    $"{CheckAndHideAlphaDomain(requestUrl, category)}";
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

        public static string FormatServerAndDatabaseInfo(string rawMessage, Machine machine)
        {
            var returnMessage = $"{Constants.NewLine}**Server:** {machine.MachineName} ({machine.MachineIP})";

            var databaseInfoIndex = rawMessage.IndexOf("DATABASE INFO", StringComparison.InvariantCultureIgnoreCase);

            if (databaseInfoIndex > 0)
            {
                var serverIndex = rawMessage.IndexOf("Server:", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var parameterIndex = rawMessage.IndexOf("Parameters:", databaseInfoIndex, StringComparison.InvariantCultureIgnoreCase);
                var databaseInfo = "No information";

                if (serverIndex > 0 && parameterIndex > 0)
                {
                    databaseInfo = rawMessage.Substring(serverIndex, parameterIndex - serverIndex);
                }

                returnMessage += $"{Constants.NewLine}**Database:**{Constants.NewLine}{databaseInfo}";
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

        public static string CheckAndHideAlphaDomain(string request, LogCategory category)
        {
            var hideDomainRequest = request;

            if (category.CategoryName.ToLowerInvariant().Contains("alpha"))
            {
                var requestUri = new Uri(request);

                hideDomainRequest = $"http://alpha.site{requestUri.AbsolutePath}";
            }

            return hideDomainRequest;
        }
    }
}