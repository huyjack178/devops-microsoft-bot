namespace Fanex.Bot.Service.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;
    using Fanex.Bot.Service.Models.Log;
    using Fanex.Bot.Service.Services;
    using Fanex.Data.Repository;

    public class LogController : ApiController
    {
#pragma warning disable S3216 // "ConfigureAwait(false)" should be used

        [HttpPost]
        public async Task<IHttpActionResult> List(GetLogCriteria criteria)
        {
            var logService = new LogService(new DynamicRepository());
            var logs = await logService.GetLogsAsync(criteria);

            return Json(logs);
        }

#pragma warning restore S3216 // "ConfigureAwait(false)" should be used
    }
}