using System;
using System.Linq;
using System.Text;
using Fanex.Bot.Core._Shared.Constants;
using Fanex.Bot.Core._Shared.Enumerations;
using Fanex.Bot.Core.Zabbix.Models;
using Fanex.Bot.Helpers;
using Fanex.Bot.Skynex._Shared.Base;

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

            message.Append($"{MessageFormatSymbol.BOLD_END}[{serviceGroup.Key}]{MessageFormatSymbol.BOLD_START}{MessageFormatSymbol.NEWLINE}");

            foreach (var service in serviceGroup.OrderByDescending(s => s.LastValue))
            {
                Enum.TryParse(service.LastValue, out ZabbixServiceStatus status);
                var statusMessage = status.ToString();

                if (status != ZabbixServiceStatus.Running)
                {
                    statusMessage = MessageFormatSymbol.BOLD_START + status.ToString() + MessageFormatSymbol.BOLD_END;
                }

                message.Append($"{service.Name} is {statusMessage}{MessageFormatSymbol.NEWLINE}");
            }

            message.Append(MessageFormatSymbol.DIVIDER + MessageFormatSymbol.NEWLINE);

            return message.ToString();
        }
    }
}