namespace Fanex.Bot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Models.Log;
    using Microsoft.Extensions.Configuration;

    public interface ILogService
    {
        Task<IEnumerable<Log>> GetErrorLogs(DateTime? fromDate = null, DateTime? toDate = null, bool isProduction = true);

        Task<Log> GetErrorLogDetail(long logId);
    }

    public class LogService : ILogService
    {
        private readonly IWebClient _webClient;
        private readonly string _mSiteUrl;

        public LogService(IWebClient webClient, IConfiguration configuration)
        {
            _webClient = webClient;
            _mSiteUrl = configuration.GetSection("LogInfo")?.GetSection("mSiteUrl")?.Value;
        }

        public async Task<IEnumerable<Log>> GetErrorLogs(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool isProduction = true)
        {
            var errorLogs = await _webClient.PostJsonAsync<GetLogFormData, IEnumerable<Log>>(
                new Uri($"{_mSiteUrl}/Bot/Logs"),
                new GetLogFormData
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
            var logMessageDetail = await _webClient.GetJsonAsync<Log>(
                new Uri($"{_mSiteUrl}/Bot/Log?logId={logId}"));

            return logMessageDetail;
        }
    }
}