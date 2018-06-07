namespace Fanex.Bot.Dialogs
{
    using System.Threading.Tasks;
    using Fanex.Bot.Models.GitLab;

    public interface IGitLabDialog : IRootDialog
    {
        Task HandlePushEventAsync(PushEvent pushEvent);
    }
}