namespace Fanex.Bot.API.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.API.Services;
    using Fanex.Bot.Core.ExecuteSP.Models;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class ExecuteSpController : ControllerBase
    {
        private readonly IExecuteSpService executeSpService;

        public ExecuteSpController(IExecuteSpService executeSpService)
        {
            this.executeSpService = executeSpService;
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        [Route("Execute")]
        public async Task<IActionResult> Execute([FromBody]ExecuteSpParam param)
        {
            var result = await executeSpService.ExecuteSP(param).ConfigureAwait(false);

            return Ok(result);
        }
    }
}