namespace Fanex.Bot.Core._Shared.Enumerations
{
    public class MessengerType : Enumeration
    {
        public const string SkypeMessengerTypeName = "skype";
        public const string TelegramMessengerTypeName = "telegram";

        public static readonly MessengerType Skype = new MessengerType(1, SkypeMessengerTypeName);

        public static readonly MessengerType Telegram = new MessengerType(2, TelegramMessengerTypeName);

        public MessengerType()
        {
        }

        private MessengerType(byte value, string displayName)
            : base(value, displayName)
        {
        }
    }
}