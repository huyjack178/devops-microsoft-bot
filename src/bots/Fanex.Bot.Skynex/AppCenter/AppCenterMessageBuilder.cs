using System;
using System.Text;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core.AppCenter.Models;
using Fanex.Bot.Helpers;
using Fanex.Bot.Skynex._Shared.Base;
using Fanex.Bot.Skynex.Sentry;
using Microsoft.Extensions.Configuration;

namespace Fanex.Bot.Skynex.AppCenter
{
    public interface IAppCenterMessageBuilder : IMessageBuilder
    {
    }

    public class AppCenterMessageBuilder : IAppCenterMessageBuilder
    {
        private readonly int defaultGMT;

        public AppCenterMessageBuilder(IConfiguration configuration)

        {
            defaultGMT = configuration.GetSection("DefaultGMT").Get<int>();
        }

        public string BuildMessage(object model)
        {
            var pushEvent = DataHelper.Parse<AppCenterEvent>(model);

            var messageBuilder = new StringBuilder();

            messageBuilder.Append(
                $"{MessageFormatSymbol.BOLD_START}Project:{MessageFormatSymbol.BOLD_END} " +
                pushEvent.Url + MessageFormatSymbol.NEWLINE);

            var logTime = pushEvent.SendAt.ToOffset(TimeSpan.FromHours(defaultGMT));

            messageBuilder.Append(
                $"{MessageFormatSymbol.BOLD_START}Timestamp:{MessageFormatSymbol.BOLD_END} {logTime}{MessageFormatSymbol.NEWLINE}");

            messageBuilder.Append(
                $"{MessageFormatSymbol.BOLD_START}Message:{MessageFormatSymbol.BOLD_END} {pushEvent.Text}{MessageFormatSymbol.NEWLINE}");

            messageBuilder.Append(MessageFormatSymbol.DIVIDER);

            return messageBuilder.ToString();
        }
    }
}