namespace Fanex.Bot.API.Controllers
{
    using Fanex.Bot.API.Services;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System.Threading.Tasks;

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

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        [Route("ListNewLog")]
        public async Task<IActionResult> ListNewLog()
        {
            var logs = await dbLogService.GetNewDbLogs();
            Logger.Log.Info(JsonConvert.SerializeObject(logs));

            return new JsonResult(logs);
        }

        [HttpPost]
        [Route("AckNewLog")]
        public async Task<IActionResult> AckNewLog([FromBody]int[] notificationIds)
        {
            await dbLogService.AckNewDbLogs(notificationIds);

            return Ok();
        }
    }
}