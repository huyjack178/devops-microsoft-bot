namespace Fanex.Bot.API.Controllers
{
    using System;
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

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("Information")]
        public async Task<IActionResult> Information()
        {
            var umInfo = await maintenanceService.GetUnderMaintenanceTime();

            if (!umInfo.IsUnderMaintenanceTime)
            {
                umInfo.From = DateTime.MinValue.ToString();
                umInfo.To = DateTime.MinValue.ToString();
            }

            return new JsonResult(umInfo);
        }
    }
}