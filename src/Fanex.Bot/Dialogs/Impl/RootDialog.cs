namespace Fanex.Bot.Dialogs.Impl
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;

    public class RootDialog : Dialog, IRootDialog
    {
        public RootDialog(
          IConfiguration configuration,
          BotDbContext dbContext)
          : base(configuration, dbContext)
        {
        }

        public async Task HandleMessageAsync(Activity activity, string message)
        {
            if (message.StartsWith("group"))
            {
                await SendAsync(activity, $"Your group id is: {activity.Conversation.Id}", notifyAdmin: false);
            }
            else if (message.StartsWith("help"))
            {
                await SendAsync(activity, GetCommandMessages(), notifyAdmin: false);
            }
            else
            {
                await SendAsync(activity, "Please send **help** to get my commands", notifyAdmin: false);
            }
        }
    }
}