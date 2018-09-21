namespace Fanex.Bot.API.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.API.Services;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Route("api/[controller]")]
    [ApiController]
    public class DbLogController : ControllerBase
    {
        private readonly IDBLogService dbLogService;

        public DbLogController(IDBLogService dbLogService)
        {
            this.dbLogService = dbLogService;
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        [Route("List")]
        public async Task<IActionResult> List()
        {
            var logs = await dbLogService.GetLogs();
            Logger.Log.Info(JsonConvert.SerializeObject(logs));
            return new JsonResult(logs);
        }

        [HttpPost]
        [Route("Ack")]
        public async Task<IActionResult> AckLog([FromBody]int[] notificationIds)
        {
            await dbLogService.AckLogs(notificationIds);

            return Ok();
        }
    }
}