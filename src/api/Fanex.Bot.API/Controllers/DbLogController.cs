namespace Fanex.Bot.API.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.API.Services;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class DbLogController : ControllerBase
    {
        private readonly IDBLogService dbLogService;

        public DbLogController(IDBLogService dbLogService)
        {
            this.dbLogService = dbLogService;
        }

        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> List()
        {
            var logs = await dbLogService.GetLogs(5);

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