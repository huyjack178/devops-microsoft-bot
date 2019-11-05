using System.Threading.Tasks;
using Fanex.Bot.Core.AppCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fanex.Bot.Skynex.AppCenter
{
    [Route("api/[controller]")]
    public class AppCenterController : Controller
    {
        private readonly IAppCenterDialog appCenterDialog;

        public AppCenterController(IAppCenterDialog appCenterDialog)
        {
            this.appCenterDialog = appCenterDialog;
        }

        [HttpPost("hook")]
        public async Task<int> Hook([FromBody]object payload)
        {
            var pushEvent = JsonConvert.DeserializeObject<AppCenterEvent>(payload.ToString());
            await appCenterDialog.HandlePushEventAsync(pushEvent);

            return 0;
        }
    }
}