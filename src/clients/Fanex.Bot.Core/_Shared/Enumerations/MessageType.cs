using System.ComponentModel;

namespace Fanex.Bot.Core._Shared.Enumerations
{
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