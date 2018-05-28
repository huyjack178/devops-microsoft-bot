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

        public async Task<string> GetErrorLog(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = await _webClient.PostAsync<GetLogFormData, IEnumerable<Log>>("PublicLog/Logs", new GetLogFormData
            {
                From = (fromDate ?? DateTime.Now.AddSeconds(-30)).AddHours(7).ToString(),
                To = (toDate ?? DateTime.Now).AddHours(7).ToString(),
                Severity = "Error",
                Size = 1,
                Page = 0,
                ToGMT = 7,
                CategoryId = 0,
                MachineId = 0,
            });

            var errorLog = result?.FirstOrDefault();

            if (errorLog != null)
            {
                return
                   $"**Category**: {errorLog.Category.CategoryName}\n\n" +
                   $"{errorLog.FullMessage}\n\n" +
                   $"\n\n----------------------------\n\n";
            }

            return string.Empty;
        }
    }
}