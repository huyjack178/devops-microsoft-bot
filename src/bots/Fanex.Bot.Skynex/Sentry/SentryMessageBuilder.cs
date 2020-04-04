using System;
using System.Text;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core.Sentry.Models;
using Fanex.Bot.Helpers;
using Fanex.Bot.Skynex._Shared.Base;
using Microsoft.Extensions.Configuration;

namespace Fanex.Bot.Skynex.Sentry
{
    public interface ISentryMessageBuilder : IMessageBuilder
    {
    }

    public class SentryMessageBuilder : ISentryMessageBuilder
    {
        private readonly int defaultGMT;

        public SentryMessageBuilder(IConfiguration configuration)

        {
            defaultGMT = configuration.GetSection("DefaultGMT").Get<int>();
        }

        public string BuildMessage(object model)
        {
            var pushEvent = DataHelper.Parse<PushEvent>(model);

            var messageBuilder = new StringBuilder();

            messageBuilder.Append(
                $"{MessageFormatSymbol.ERROR}{pushEvent.Level.ToUpperInvariant()}{MessageFormatSymbol.ERROR} in " +
                $"{MessageFormatSymbol.BOLD_START}「{pushEvent.ProjectName.ToUpperInvariant()}」{MessageFormatSymbol.BOLD_END}{MessageFormatSymbol.NEWLINE}");

            var logTime = DateTimeOffset
                .FromUnixTimeSeconds(Convert.ToInt64(Convert.ToDouble(pushEvent.Event.LogTime)))
                .ToOffset(TimeSpan.FromHours(defaultGMT));

            messageBuilder.Append(
                $"{MessageFormatSymbol.BOLD_START}Timestamp:{MessageFormatSymbol.BOLD_END} " +
                $"{logTime}" +
                $"{MessageFormatSymbol.NEWLINE}");
            messageBuilder.Append(
                $"{MessageFormatSymbol.BOLD_START}Environment:{MessageFormatSymbol.BOLD_END} " +
                $"{pushEvent.Event.Environment}{MessageFormatSymbol.NEWLINE}");
            messageBuilder.Append(
                $"{MessageFormatSymbol.BOLD_START}Message:{MessageFormatSymbol.BOLD_END} " +
                $"{pushEvent.Event.Message}{MessageFormatSymbol.NEWLINE}");

            if (pushEvent.Event.Request != null)
            {
                var request = pushEvent.Event.Request;
                messageBuilder.Append(
                    $"{MessageFormatSymbol.BOLD_START}Request:{MessageFormatSymbol.BOLD_END} [{request.Method}] {request.Url}{MessageFormatSymbol.NEWLINE}");
            }

            if (pushEvent.Event.Context?.Browser != null)
            {
                var browser = pushEvent.Event.Context.Browser;
                messageBuilder.Append(
                    $"{MessageFormatSymbol.BOLD_START}Browser:{MessageFormatSymbol.BOLD_END} {browser.Name} ({browser.Version}){MessageFormatSymbol.NEWLINE}");
            }

            if (pushEvent.Event.Context?.Device != null)
            {
                var device = pushEvent.Event.Context.Device;
                messageBuilder.Append(
                    $"{MessageFormatSymbol.BOLD_START}Device:{MessageFormatSymbol.BOLD_END} {device.Name} ({device.Model}){MessageFormatSymbol.NEWLINE}");
            }

            if (pushEvent.Event.Context?.Os != null)
            {
                var os = pushEvent.Event.Context.Os;
                messageBuilder.Append(
                    $"{MessageFormatSymbol.BOLD_START}OS:{MessageFormatSymbol.BOLD_END} {os.Name} {os.Version}{MessageFormatSymbol.NEWLINE}");
            }

            if (pushEvent.Event.User?.UserName != null)
            {
                messageBuilder.Append(
                  $"{MessageFormatSymbol.BOLD_START}User:{MessageFormatSymbol.BOLD_END} " +
                  $"{pushEvent.Event.User.UserName}{MessageFormatSymbol.NEWLINE}");
            }

            if (!string.IsNullOrEmpty(pushEvent.Event.User?.IpAddress))
            {
                messageBuilder.Append(
                    $"IP Address: {pushEvent.Event.User.IpAddress}{MessageFormatSymbol.NEWLINE}");
            }

            if (!string.IsNullOrEmpty(pushEvent.Event.User?.Email))
            {
                messageBuilder.Append(
                    $"Email: {pushEvent.Event.User.Email}{MessageFormatSymbol.NEWLINE}");
            }

            messageBuilder.Append($"{MessageFormatSymbol.NEWLINE}");
            messageBuilder.Append(
              "For more detail, refer to " +
              $"[here]({pushEvent.Url})" +
              $"{MessageFormatSymbol.NEWLINE}");
            messageBuilder.Append($"{MessageFormatSymbol.DIVIDER}");

            return messageBuilder.ToString();
        }
    }
}