namespace Fanex.Bot.API.Services
{
    using Fanex.Bot.API.DbParams.Commands;
    using Fanex.Bot.API.DbParams.Criterias;
    using Fanex.Bot.API.Models.Log;
    using Fanex.Data.Repository;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDBLogService
    {
        Task<IEnumerable<DBLog>> GetLogs();

        Task AckLogs(int[] logNotificationIds);

        Task<IEnumerable<DBLog>> GetNewDbLogs();

        Task AckNewDbLogs(int[] logNotificationIds);
    }

    public class DBLogService : IDBLogService
    {
        private readonly IDynamicRepository dynamicRepository;

        public DBLogService(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
        }

        public Task<IEnumerable<DBLog>> GetLogs()
        {
            return dynamicRepository.FetchAsync<DBLog>(new GetDbLogCriteria());
        }

        public async Task AckLogs(int[] logNotificationIds)
        {
            await dynamicRepository.ExecuteAsync(new AckDbLogCommand(logNotificationIds));
        }

        public async Task<IEnumerable<DBLog>> GetNewDbLogs()
        {
            return await dynamicRepository.FetchAsync<DBLog>(new GetNewDbLogCriteria());
        }

        public async Task AckNewDbLogs(int[] logNotificationIds)
        {
            await dynamicRepository.ExecuteAsync(new AckNewDbLogCommand(logNotificationIds));
        }
    }
}