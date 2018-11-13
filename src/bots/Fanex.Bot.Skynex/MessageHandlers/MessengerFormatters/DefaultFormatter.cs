using System.Net;

namespace Fanex.Bot.Skynex.MessageHandlers.MessengerFormatters
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
                .Replace(MessageFormatSignal.NewLine, NewLine)
                .Replace(MessageFormatSignal.DoubleNewLine, DoubleNewLine)
                .Replace(MessageFormatSignal.BeginBold, BeginBold)
                .Replace(MessageFormatSignal.EndBold, EndBold)
                .Replace(MessageFormatSignal.BreakLine, BreakLine);

        protected virtual string Clean(string message)
            => message
                .Replace("\n\n \n\n", NewLine)
                .Replace("\n\n\n\n", NewLine)
                .Replace("\n\n\n", NewLine)
                .Replace("\n\n", NewLine)
                .Replace("\n \n", NewLine);
    }
}