namespace Fanex.Bot.Skynex.MessageHandlers.MessengerFormatters
{
    public interface ILineFormatter : IMessengerFormatter
    {
    }

    public class LineFormatter : DefaultFormatter, ILineFormatter
    {
        public override string Format(string message)
            => Clean(message)
                .Replace(MessageFormatSignal.NEWLINE, NewLine)
                .Replace(MessageFormatSignal.DOUBLE_NEWLINE, DoubleNewLine)
                .Replace(MessageFormatSignal.BOLD_START, string.Empty)
                .Replace(MessageFormatSignal.BOLD_END, string.Empty)
                .Replace(MessageFormatSignal.DIVIDER, BreakLine);
    }
}