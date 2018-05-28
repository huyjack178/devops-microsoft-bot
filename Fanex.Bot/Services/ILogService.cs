namespace Fanex.Bot.Services
{
    using System;
    using System.Threading.Tasks;

    public interface ILogService
    {
        Task<string> GetErrorLog(DateTime? fromDate = null, DateTime? toDate = null);
    }
}