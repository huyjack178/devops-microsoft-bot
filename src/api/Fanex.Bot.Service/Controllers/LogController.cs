namespace Fanex.Bot.Service.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using Fanex.Bot.Service.Models.Log;
    using Fanex.Bot.Service.Services;

    public class LogController : ApiController
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

#pragma warning disable S3216 // "ConfigureAwait(false)" should be used

        [HttpPost]
        public async Task<IHttpActionResult> List(GetLogCriteria criteria)
        {
            var logs = await _logService.GetLogsAsync(criteria);

            return Json(logs);
        }

#pragma warning restore S3216 // "ConfigureAwait(false)" should be used
    }
}