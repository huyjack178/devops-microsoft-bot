namespace Fanex.Bot.Client.Enums
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
}