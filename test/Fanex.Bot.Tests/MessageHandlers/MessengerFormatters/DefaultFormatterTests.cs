namespace Fanex.Bot.Skynex.Tests.MessageHandlers.MessengerFormatters
{
    using Fanex.Bot.Skynex.MessageHandlers.MessengerFormatters;
    using Xunit;

    public class DefaultFormatterTests
    {
        private readonly IMessengerFormatter defautFormatter;

        public DefaultFormatterTests()
        {
            defautFormatter = new DefaultFormatter();
        }

        [Fact]
        public void NewLine_Always_GetDefaultValue()
        {
            // Act
            var newLine = defautFormatter.NewLine;

            // Assert
            Assert.Equal("\n", newLine);
        }

        [Fact]
        public void DoubleNewLine_Always_GetDefaultValue()
        {
            // Act
            var doubleNewLine = defautFormatter.DoubleNewLine;

            // Assert
            Assert.Equal("\n\n", doubleNewLine);
        }

        [Fact]
        public void BeginBold_Always_GetDefaultValue()
        {
            // Act
            var beginBold = defautFormatter.BeginBold;

            // Assert
            Assert.Equal("**", beginBold);
        }

        [Fact]
        public void EndBold_Always_GetDefaultValue()
        {
            // Act
            var endBold = defautFormatter.EndBold;

            // Assert
            Assert.Equal("**", endBold);
        }

        [Fact]
        public void BreakLine_Always_GetDefaultValue()
        {
            // Act
            var breakLine = defautFormatter.BreakLine;

            // Assert
            Assert.Equal("***", breakLine);
        }

        [Fact]
        public void Format_Always_ReturnFormatText()
        {
            // Arrange
            var text =
                $"{MessageFormatSignal.BeginBold}Hello: {MessageFormatSignal.EndBold}\n\n \n\n" +
                $"{MessageFormatSignal.NewLine}{MessageFormatSignal.DoubleNewLine}" +
                $"{MessageFormatSignal.BreakLine}";

            // Act
            var formatedText = defautFormatter.Format(text);

            // Assert
            var expectedFormatedText = "**Hello: **\n\n\n\n***";
            Assert.Equal(expectedFormatedText, formatedText);
        }
    }
}