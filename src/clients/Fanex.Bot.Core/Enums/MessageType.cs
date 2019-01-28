namespace Fanex.Bot.Enums
{
    using System.ComponentModel;

    public enum MessageType
    {
        [Description("markdown")]
        Markdown = 1,

        [Description("xml")]
        XML = 2,

        [Description("plain")]
        Plain = 3,
    }

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