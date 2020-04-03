﻿namespace Fanex.Bot.Skynex._Shared.MessengerFormatters
{
    public interface ITelegramFormatter : IMessageFormatter
    {
    }

    public class TelegramFormatter : SkypeFormatter, ITelegramFormatter
    {
        public override string NewLine { get; } = "\n\n";

        public override string Bell => ":bell:";
    }
}