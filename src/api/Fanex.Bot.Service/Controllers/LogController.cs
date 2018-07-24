namespace Fanex.Bot.Service.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Fanex.Bot.Service.Models.Log;
    using Fanex.Bot.Service.Services;
    using Fanex.Data.Repository;

    public class LogController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> List(GetLogCriteria criteria)
        {
            var logService = new LogService(new DynamicRepository());
            var logs = await logService.GetLogsAsync(criteria);

            return Json(logs);
        }
    }
}