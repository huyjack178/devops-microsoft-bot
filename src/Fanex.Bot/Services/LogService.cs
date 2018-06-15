namespace Fanex.Bot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Models.Log;
    using Fanex.Bot.Utilitites;

    public class LogService : ILogService
    {
        private readonly IWebClient _webClient;

        public LogService(IWebClient webClient)
        {
            _webClient = webClient;
        }

        public async Task<IEnumerable<Log>> GetErrorLogs(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool isProduction = true)
        {
            var errorLogs = await _webClient.PostAsync<GetLogFormData, IEnumerable<Log>>("PublicLog/Logs", new GetLogFormData
            {
                From = (fromDate ?? DateTime.UtcNow.AddSeconds(-70)).AddHours(7).ToString(CultureInfo.InvariantCulture),
                To = (toDate ?? DateTime.UtcNow).AddHours(7).ToString(CultureInfo.InvariantCulture),
                Severity = "Error",
                Size = 5,
                Page = 0,
                ToGMT = 7,
                CategoryId = 0,
                MachineId = 0,
                IsProduction = isProduction
            });

            return errorLogs.Any() ? errorLogs : new List<Log>();
        }

        public async Task<Log> GetErrorLogDetail(long logId)
        {
            var logMessageDetail = await _webClient.GetAsync<Log>($"PublicLog/Log?logId={logId}");

            return logMessageDetail;
        }
    }
}