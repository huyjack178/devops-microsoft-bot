namespace Fanex.Bot.API.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.API.Services;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogService logService;

        public LogController(ILogService logService)
        {
            this.logService = logService;
        }

#pragma warning disable S3216 // "ConfigureAwait(false)" should be used

        [HttpPost]
        [Route("List")]
        public async Task<IActionResult> List(GetLogCriteria criteria)
        {
            var logs = await logService.GetLogsAsync(criteria);

            return new JsonResult(logs);
        }
    }
}