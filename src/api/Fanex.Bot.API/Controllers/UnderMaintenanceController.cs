namespace Fanex.Bot.API.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using ONEbook.UM.Services;

    [Route("api/[controller]")]
    [ApiController]
    public class UnderMaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService maintenanceService;

        public UnderMaintenanceController(IMaintenanceService maintenanceService)
        {
            this.maintenanceService = maintenanceService;
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("ScheduledInfo")]
        public async Task<IActionResult> ScheduledInfo()
        {
            var umInfos = await maintenanceService.GetScheduledUnderMaintenance();

            return new JsonResult(umInfos);
        }

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("ActualInfo")]
        public async Task<IActionResult> ActualInfo()
        {
            var umInfos = await maintenanceService.GetAllUnderMaintenanceTime();

            return new JsonResult(umInfos);
        }
    }
}