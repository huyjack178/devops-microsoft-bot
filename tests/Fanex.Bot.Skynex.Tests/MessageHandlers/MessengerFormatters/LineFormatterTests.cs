namespace Fanex.Bot.Skynex.Tests.MessageHandlers.MessengerFormatters
{
    using Fanex.Bot.Skynex.MessageHandlers.MessengerFormatters;
    using Xunit;

    public class LineFormatterTests
    {
        private readonly ILineFormatter lineFormatter;

        public LineFormatterTests()
        {
            lineFormatter = new LineFormatter();
        }

        [Fact]
        public void Format_Always_GetFormatedText()
        {
            // Arrange
            var text = $"{MessageFormatSignal.NEWLINE}Hello: {MessageFormatSignal.BOLD_START}{MessageFormatSignal.BOLD_END}";

            // Act
            var formatedText = lineFormatter.Format(text);

            // Assert
            var expectedFormatedText = "\nHello: ";
            Assert.Equal(expectedFormatedText, formatedText);
        }
    }
}