namespace Fanex.Bot.Skynex.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Filters;
    using Fanex.Bot.Models.GitLab;
    using Fanex.Bot.Skynex.Dialogs;
    using Microsoft.AspNetCore.Mvc;

    [ServiceFilter(typeof(GitLabAttribute))]
    [Route("api/[controller]")]
    public class GitLabWebHookController : Controller
    {
        private readonly IGitLabDialog gitLabDialog;

        public GitLabWebHookController(IGitLabDialog gitLabDialog)
        {
            this.gitLabDialog = gitLabDialog;
        }

        [HttpPost]
        public async Task<int> PushEventInfo([FromBody]PushEvent pushEvent)
        {
            await gitLabDialog.HandlePushEventAsync(pushEvent);

            return 0;
        }
    }
}