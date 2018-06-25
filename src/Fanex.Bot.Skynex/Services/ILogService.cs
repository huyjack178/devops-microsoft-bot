namespace Fanex.Bot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Bot.Models.Log;

    public interface ILogService
    {
        Task<IEnumerable<Log>> GetErrorLogs(DateTime? fromDate = null, DateTime? toDate = null, bool isProduction = true);

        Task<Log> GetErrorLogDetail(long logId);
    }
}