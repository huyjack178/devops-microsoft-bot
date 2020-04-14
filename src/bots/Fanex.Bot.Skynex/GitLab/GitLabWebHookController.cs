using System;
using System.Threading.Tasks;
using Fanex.Bot.Core.GitLab.Models;
using Fanex.Bot.Core.GitLab.Models.JobEvents;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
        public async Task<int> Handle([FromBody]object data)
        {
            var gitlabData = JsonConvert.DeserializeObject<GitlabEvent>(data.ToString());

            try
            {
                if (gitlabData.EventType.IsPush)
                {
                    var pushEvent = JsonConvert.DeserializeObject<PushEvent>(data.ToString());
                    await gitLabDialog.HandlePushEventAsync(pushEvent);

                    return 0;
                }

                if (gitlabData.EventType.IsJob)
                {
                    var jobEvent = JsonConvert.DeserializeObject<JobEvent>(data.ToString());
                    await gitLabDialog.HandleJobEventAsync(jobEvent);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Gitlab Error {data} \n {ex.Message}", ex);
            }

            return 0;
        }
    }
}