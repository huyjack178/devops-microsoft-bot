using Fanex.Bot.Core._Shared.Constants;

namespace Fanex.Bot.Skynex._Shared.MessengerFormatters
{
    public interface IMessengerFormatter
    {
        string NewLine { get; }

        string DoubleNewLine { get; }

        string BeginBold { get; }

        string EndBold { get; }

        string BreakLine { get; }

        string Format(string message);
    }

    public class DefaultFormatter : IMessengerFormatter
    {
        public string NewLine { get; } = "\n";

        public string DoubleNewLine { get; } = "\n\n";

        public string BeginBold { get; } = "**";

        public string EndBold { get; } = "**";

        public string BreakLine { get; } = "***";

        public virtual string Format(string message)
            => Clean(message)
                .Replace(MessageFormatSignal.NEWLINE, NewLine)
                .Replace(MessageFormatSignal.DOUBLE_NEWLINE, DoubleNewLine)
                .Replace(MessageFormatSignal.BOLD_START, BeginBold)
                .Replace(MessageFormatSignal.BOLD_END, EndBold)
                .Replace(MessageFormatSignal.DIVIDER, BreakLine);

        protected virtual string Clean(string message)
            => message
                .Replace("\n\n \n\n", NewLine)
                .Replace("\n\n\n\n", NewLine)
                .Replace("\n\n\n", NewLine)
                .Replace("\n\n", NewLine)
                .Replace("\n \n", NewLine)
                .Replace("<br>", NewLine);
    }
}