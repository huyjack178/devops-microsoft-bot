namespace Fanex.Bot.Service.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using ONEbook.UM.Services;

    [Route("api/[controller]")]
    [ApiController]
    public class UMController : ControllerBase
    {
        private readonly IMaintenanceService maintenanceService;

        public UMController(IMaintenanceService maintenanceService)
        {
            this.maintenanceService = maintenanceService;
        }

        [HttpGet]
        [Route("Information")]
        public async Task<IActionResult> Information()
        {
            var umInfo = await maintenanceService.GetUnderMaintenanceTime();

            return new JsonResult(umInfo);
        }
    }
}