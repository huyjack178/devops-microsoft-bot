namespace Fanex.Bot.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Filters;
    using Fanex.Bot.Models.GitLab;
    using Microsoft.AspNetCore.Mvc;

    [ServiceFilter(typeof(GitLabAttribute))]
    [Route("api/[controller]")]
    public class GitLabWebHookController : Controller
    {
        private readonly IGitLabDialog _gitLabDialog;

        public GitLabWebHookController(IGitLabDialog gitLabDialog)
        {
            _gitLabDialog = gitLabDialog;
        }

        [HttpPost]
        public async Task<int> PushEventInfo([FromBody]PushEvent pushEvent)
        {
            await _gitLabDialog.HandlePushEventAsync(pushEvent);

            return 0;
        }
    }
}