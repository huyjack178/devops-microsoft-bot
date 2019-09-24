using System.Threading.Tasks;
using Fanex.Bot.Core.GitLab.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Bot.Skynex.GitLab
{
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