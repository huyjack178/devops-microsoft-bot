using System.Linq;
using System.Xml.Linq;

namespace Fanex.Bot.Common.Helpers.Bot
{
    public static class BotHelper
    {
        public static string GenerateMessage(string message, string[] botNames)
        {
            if (botNames?.Any() != true)
            {
                return message;
            }

            var formattedMessage = message;

            foreach (var botName in botNames)
            {
                formattedMessage = formattedMessage.Replace(botName, string.Empty);
            }

            return formattedMessage
                .Replace("@", string.Empty)
                .Trim()
                .ToLowerInvariant();
        }

#pragma warning disable S3994 // URI Parameters should not be strings

        public static string ExtractProjectLink(string projectUrl)
        {
            string extractProjectLink;

            try
            {
                extractProjectLink = XElement.Parse(projectUrl).Attribute("href").Value;
            }
            catch
            {
                extractProjectLink = string.Empty;
            }

            if (string.IsNullOrEmpty(extractProjectLink))
            {
                extractProjectLink = projectUrl;
            }

            return extractProjectLink;
        }

#pragma warning restore S3994 // URI Parameters should not be strings
    }
}