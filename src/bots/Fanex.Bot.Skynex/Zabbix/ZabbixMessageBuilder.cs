using System;
using System.Linq;
using System.Text;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Zabbix.Models;
using Fanex.Bot.Helpers;
using Fanex.Bot.Skynex.Log;

namespace Fanex.Bot.Skynex.Zabbix
{
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

            foreach (var service in serviceGroup.OrderByDescending(s => s.LastValue))
            {
                Enum.TryParse(service.LastValue, out ZabbixServiceStatus status);
                var statusMessage = status.ToString();

                if (status != ZabbixServiceStatus.Running)
                {
                    statusMessage = MessageFormatSignal.BOLD_START + status.ToString() + MessageFormatSignal.BOLD_END;
                }

                message.Append($"{service.Name} is {statusMessage}{MessageFormatSignal.NEWLINE}");
            }

            message.Append(MessageFormatSignal.DIVIDER + MessageFormatSignal.NEWLINE);

            return message.ToString();
        }
    }
}