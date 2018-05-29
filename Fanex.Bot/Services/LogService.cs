namespace Fanex.Bot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models;
    using Fanex.Bot.Utilitites;

    public class LogService : ILogService
    {
        private readonly IWebClient _webClient;

        public LogService(IWebClient webClient)
        {
            _webClient = webClient;
        }

        public async Task<IEnumerable<Log>> GetErrorLogs(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var errorLogs = await _webClient.PostAsync<GetLogFormData, IEnumerable<Log>>("PublicLog/Logs", new GetLogFormData
            {
                From = (fromDate ?? DateTime.UtcNow.AddSeconds(-70)).AddHours(7).ToString(),
                To = (toDate ?? DateTime.UtcNow).AddHours(7).ToString(),
                Severity = "Error",
                Size = 5,
                Page = 0,
                ToGMT = 7,
                CategoryId = 0,
                MachineId = 0,
                IsProduction = true
            });

            return errorLogs.Any() ? errorLogs : new List<Log>();
        }
    }
}