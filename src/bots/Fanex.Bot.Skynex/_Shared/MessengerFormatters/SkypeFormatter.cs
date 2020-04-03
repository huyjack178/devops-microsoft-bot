﻿using Fanex.Bot.Core._Shared.Constants;

namespace Fanex.Bot.Skynex._Shared.MessengerFormatters
{
    public interface IMessageFormatter
    {
        string NewLine { get; }

        string DoubleNewLine { get; }

        string BeginBold { get; }

        string EndBold { get; }

        string BreakLine { get; }

        string Format(string message);
    }

    public class SkypeFormatter : IMessageFormatter
    {
        public virtual string NewLine { get; } = "\n";

        public virtual string DoubleNewLine { get; } = "\n\n";

        public virtual string BeginBold { get; } = "**";

        public virtual string EndBold { get; } = "**";

        public virtual string BreakLine { get; } = "***";

        public virtual string Format(string message)
            => Clean(message)
                .Replace(MessageFormatSymbol.NEWLINE, NewLine)
                .Replace(MessageFormatSymbol.DOUBLE_NEWLINE, DoubleNewLine)
                .Replace(MessageFormatSymbol.BOLD_START, BeginBold)
                .Replace(MessageFormatSymbol.BOLD_END, EndBold)
                .Replace(MessageFormatSymbol.DIVIDER, BreakLine);

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