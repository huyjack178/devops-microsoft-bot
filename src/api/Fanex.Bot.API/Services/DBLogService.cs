namespace Fanex.Bot.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Bot.API.Models.Log;

    public interface IDBLogService
    {
        Task<IEnumerable<DBLog>> GetLogs(int rangeMinute);

        Task AckLogs(string[] logNotificationIds);
    }

    public class DBLogService : IDBLogService
    {
        public Task<IEnumerable<DBLog>> GetLogs(int rangeMinute)
        {
            throw new NotImplementedException();
        }

        public Task AckLogs(string[] logNotificationIds)
        {
            throw new NotImplementedException();
        }
    }
}