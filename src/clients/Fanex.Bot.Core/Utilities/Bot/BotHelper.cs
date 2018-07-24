namespace Fanex.Bot.Core.Utilities.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public static class BotHelper
    {
        public static string GenerateMessage(string message)
        {
            var returnMessage = message;

            if (message.StartsWith("@"))
            {
                var indexOfCommand = message.IndexOf(' ');

                if (indexOfCommand > 0)
                {
                    returnMessage = message.Remove(0, indexOfCommand).Trim();
                }
            }

            return returnMessage.ToLowerInvariant();
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