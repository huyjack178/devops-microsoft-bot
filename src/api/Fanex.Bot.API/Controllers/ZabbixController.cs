namespace Fanex.Bot.API.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.API.Services;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class ZabbixController : ControllerBase
    {
        private readonly IZabbixService zabbixService;

        public ZabbixController(IZabbixService zabbixService)
        {
            this.zabbixService = zabbixService;
        }

#pragma warning disable S3216 // "ConfigureAwait(false)" should be used

        [HttpPost]
        [Route("services")]
        public async Task<IActionResult> GetServices([FromBody]string[] serviceKeys)
        {
            var services = await zabbixService.GetServices(serviceKeys);

            return new JsonResult(services);
        }
    }
}