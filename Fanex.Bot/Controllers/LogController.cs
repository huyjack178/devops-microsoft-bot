namespace Fanex.Bot.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Services;
    using Microsoft.AspNetCore.Mvc;

    public class LogController : Controller
    {
        private readonly ILogService logService;

        public LogController(ILogService logService)
        {
            this.logService = logService;
        }

        //public IActionResult SyncLogsToClient()
        //{
        //}
    }
}