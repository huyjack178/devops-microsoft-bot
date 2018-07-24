namespace Fanex.Bot.Service.Controllers
{
    using System;
    using System.Globalization;
    using System.Web.Http;
    using ONELab.UMService.UMClient;

    public class UMController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Information()
        {
            var result = WebSiteClient.IsUnderUM(out DateTime startTime, out DateTime endTime, out int errorCode);

            return Json(new
            {
                isUM = result,
                WebSiteClient.VersionChkMessage,
                startTime = startTime.ToString(CultureInfo.InvariantCulture),
                endTime = endTime.ToString(CultureInfo.InvariantCulture),
                errorCode
            });
        }
    }
}