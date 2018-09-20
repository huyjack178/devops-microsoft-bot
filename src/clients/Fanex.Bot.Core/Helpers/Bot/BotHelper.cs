namespace Fanex.Bot.Core.Utilities.Bot
{
    using System.Xml.Linq;

    public static class BotHelper
    {
        public static string GenerateMessage(string message, string botName)
        {
            if (string.IsNullOrEmpty(botName))
            {
                return message;
            }

            return message
                .Replace(botName, string.Empty)
                .Replace("@", string.Empty)
                .Trim()
                .ToLowerInvariant();
        }

#pragma warning disable S3994 // URI Parameters should not be strings

        public static string ExtractProjectLink(string projectUrl)
        {
            string formatedProjectUrl;

            try
            {
                formatedProjectUrl = XElement.Parse(projectUrl).Attribute("href").Value;
            }
            catch
            {
                formatedProjectUrl = string.Empty;
            }

            if (string.IsNullOrEmpty(formatedProjectUrl))
            {
                formatedProjectUrl = projectUrl;
            }

            return formatedProjectUrl;
        }

#pragma warning restore S3994 // URI Parameters should not be strings
    }
}