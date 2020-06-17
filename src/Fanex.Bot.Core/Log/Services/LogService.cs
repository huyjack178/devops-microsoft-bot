using Fanex.Bot.Common.Helpers.Web;
using Fanex.Bot.Core.Bot.Models;
using Fanex.Bot.Core.Log.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fanex.Bot.Core.Log.Services
{
    public interface ILogService
    {
        Task<IEnumerable<Models.Log>> GetErrorLogs(DateTime? fromDate = null, DateTime? toDate = null, bool isProduction = true);

        Task<Models.Log> GetErrorLogDetail(long logId);

        Task<IEnumerable<DBLog>> GetDBLogs();

        Task<Result> AckDBLog(int[] notificationIds);

        Task<IEnumerable<DBLog>> GetNewDBLogs();

        Task<Result> AckNewDBLog(int[] notificationIds);
    }

    public class LogService : ILogService
    {
        private readonly IWebClient webClient;
        private readonly string botServiceUrl;
        private readonly int logSize;

        public LogService(
            IWebClient webClient,
            IConfiguration configuration)
        {
            this.webClient = webClient;
            botServiceUrl = configuration.GetSection("BotServiceUrl")?.Value;
            logSize = configuration.GetValue<int>("LogInfo:Size");
        }

        public async Task<IEnumerable<Models.Log>> GetErrorLogs(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool isProduction = true)
        {
            var errorLogs = await webClient.PostJsonAsync<GetLogFormData, IEnumerable<Models.Log>>(
                new Uri($"{botServiceUrl}/Log/List"),
                new GetLogFormData
                {
                    From = (fromDate ?? DateTime.UtcNow.AddSeconds(-70)).ToString(CultureInfo.InvariantCulture),
                    To = (toDate ?? DateTime.UtcNow).ToString(CultureInfo.InvariantCulture),
                    Severity = "Error",
                    Size = logSize,
                    Page = 0,
                    ToGMT = 7,
                    CategoryId = 0,
                    MachineId = 0,
                    IsProduction = isProduction
                }).ConfigureAwait(false);

            return errorLogs.Any() ? errorLogs : new List<Models.Log>();
        }

        public Task<Models.Log> GetErrorLogDetail(long logId)
            => webClient.GetJsonAsync<Models.Log>(new Uri($"{botServiceUrl}/Log?logId={logId}"));

        public Task<IEnumerable<DBLog>> GetDBLogs()
            => webClient.PostJsonAsync<string, IEnumerable<DBLog>>(new Uri($"{botServiceUrl}/DbLog/List"), string.Empty);

        public async Task<Result> AckDBLog(int[] notificationIds)
        {
            var statusCode = await webClient
                .PostJsonAsync(new Uri($"{botServiceUrl}/DbLog/Ack"), notificationIds)
                .ConfigureAwait(false);

            return statusCode == HttpStatusCode.OK ? Result.CreateSuccessfulResult() : Result.CreateFailedResult();
        }

        public Task<IEnumerable<DBLog>> GetNewDBLogs()
            => webClient.PostJsonAsync<string, IEnumerable<DBLog>>(new Uri($"{botServiceUrl}/DbLog/ListNewLog"), string.Empty);

        public async Task<Result> AckNewDBLog(int[] notificationIds)
        {
            var statusCode = await webClient
                .PostJsonAsync(new Uri($"{botServiceUrl}/DbLog/AckNewLog"), notificationIds)
                .ConfigureAwait(false);

            return statusCode == HttpStatusCode.OK ? Result.CreateSuccessfulResult() : Result.CreateFailedResult();
        }
    }
}