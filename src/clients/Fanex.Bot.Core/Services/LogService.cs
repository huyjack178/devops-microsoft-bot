namespace Fanex.Bot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Models;
    using Fanex.Bot.Models.Log;
    using Microsoft.Extensions.Configuration;

    public interface ILogService
    {
        Task<IEnumerable<Log>> GetErrorLogs(DateTime? fromDate = null, DateTime? toDate = null, bool isProduction = true);

        Task<Log> GetErrorLogDetail(long logId);

        Task<IEnumerable<DBLog>> GetDBLogs();

        Task<Result> AckDBLog(int[] notificationIds);
    }

    public class LogService : ILogService
    {
        private readonly IWebClient webClient;
        private readonly string botServiceUrl;

        public LogService(
            IWebClient webClient,
            IConfiguration configuration)
        {
            this.webClient = webClient;
            botServiceUrl = configuration.GetSection("BotServiceUrl")?.Value;
        }

        public async Task<IEnumerable<Log>> GetErrorLogs(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool isProduction = true)
        {
            var errorLogs = await webClient.PostJsonAsync<GetLogFormData, IEnumerable<Log>>(
                new Uri($"{botServiceUrl}/Log/List"),
                new GetLogFormData
                {
                    From = (fromDate ?? DateTime.UtcNow.AddSeconds(-65)).ToString(CultureInfo.InvariantCulture),
                    To = (toDate ?? DateTime.UtcNow).ToString(CultureInfo.InvariantCulture),
                    Severity = "Error",
                    Size = 5,
                    Page = 0,
                    ToGMT = 7,
                    CategoryId = 0,
                    MachineId = 0,
                    IsProduction = isProduction
                }).ConfigureAwait(false);

            return errorLogs.Any() ? errorLogs : new List<Log>();
        }

        public Task<Log> GetErrorLogDetail(long logId)
            => webClient.GetJsonAsync<Log>(new Uri($"{botServiceUrl}/Log?logId={logId}"));

        public Task<IEnumerable<DBLog>> GetDBLogs()
            => webClient.GetJsonAsync<IEnumerable<DBLog>>(new Uri($"{botServiceUrl}/DbLog/List"));

        public async Task<Result> AckDBLog(int[] notificationIds)
        {
            var statusCode = await webClient
                .PostJsonAsync(new Uri($"{botServiceUrl}/DbLog/Ack"), notificationIds)
                .ConfigureAwait(false);

            return statusCode == HttpStatusCode.OK ? Result.CreateSuccessfulResult() : Result.CreateFailedResult();
        }
    }
}