namespace Fanex.Bot.Skynex.MessageHandlers.MessengerFormatters
{
    public interface ILineFormatter : IMessengerFormatter
    {
    }

    public class LineFormatter : DefaultFormatter, ILineFormatter
    {
        public override string Format(string message)
            => Clean(message)
                .Replace(MessageFormatSignal.NewLine, NewLine)
                .Replace(MessageFormatSignal.DoubleNewLine, DoubleNewLine)
                .Replace(MessageFormatSignal.BeginBold, string.Empty)
                .Replace(MessageFormatSignal.EndBold, string.Empty)
                .Replace(MessageFormatSignal.BreakLine, BreakLine);
    }
}