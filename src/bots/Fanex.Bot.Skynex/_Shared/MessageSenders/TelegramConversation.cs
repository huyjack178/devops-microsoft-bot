using Fanex.Bot.Skynex._Shared.MessengerFormatters;
using Microsoft.Extensions.Configuration;

namespace Fanex.Bot.Skynex._Shared.MessageSenders
{
    public interface ITelegramConversation : IMessengerConversation
    {
    }

    public class TelegramConversation : SkypeConversation, ITelegramConversation
    {
        public TelegramConversation(IConfiguration configuration, ITelegramFormatter messageFormatter)
            : base(configuration, messageFormatter)
        {
        }
    }
}