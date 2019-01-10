namespace Fanex.Bot.Skynex.MessageHandlers.MessageBuilders
{
    using System;
    using System.Linq;
    using System.Text;
    using Fanex.Bot.Enums;
    using Fanex.Bot.Helpers;
    using Fanex.Bot.Models.Zabbix;

    public interface IZabbixMessageBuilder : IMessageBuilder
    {
    }

    public class ZabbixMessageBuilder : IZabbixMessageBuilder
    {
        public string BuildMessage(object model)
        {
            var message = new StringBuilder();
            var serviceGroup = DataHelper.Parse<IGrouping<string, Service>>(model);

            message.Append($"{MessageFormatSignal.BOLD_END}[{serviceGroup.Key}]{MessageFormatSignal.BOLD_START}{MessageFormatSignal.NEWLINE}");

            foreach (var service in serviceGroup)
            {
                Enum.TryParse(service.Status, out ZabbixServiceStatus status);
                var statusMessage = status.ToString();

                if (status != ZabbixServiceStatus.Running)
                {
                    statusMessage = MessageFormatSignal.BOLD_START + status.ToString() + MessageFormatSignal.BOLD_END;
                }

                message.Append($"{MessageFormatSignal.BOLD_START}{service.Name}{MessageFormatSignal.BOLD_END} is {statusMessage}{MessageFormatSignal.NEWLINE}");
            }

            message.Append(MessageFormatSignal.DIVIDER + MessageFormatSignal.NEWLINE);

            return message.ToString();
        }
    }
}