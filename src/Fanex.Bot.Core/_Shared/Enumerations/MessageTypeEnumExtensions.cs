using System.ComponentModel;

namespace Fanex.Bot.Core._Shared.Enumerations
{
    public static class MessageTypeEnumExtensions
    {
        public static string ToDescriptionString(this MessageType val)
        {
            var attributes = (DescriptionAttribute[])val
                .GetType()
                .GetField(val.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}