namespace Fanex.Bot.Controllers
{
    using Fanex.Bot.Models.GitLab;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    public class GitLabWebHookController : Controller
    {
        [HttpPost]
        public int PushEventInfo([FromBody]PushEvent push)
        {
            //var projectUrl = push.Project.WebUrl;

            return 0;
        }
    }
}