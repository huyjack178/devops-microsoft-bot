namespace Fanex.Bot.API.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fanex.Bot.API.DbParams.Commands;
    using Fanex.Bot.API.DbParams.Criterias;
    using Fanex.Bot.API.Models.Log;
    using Fanex.Data.Repository;

    public interface IDBLogService
    {
        Task<IEnumerable<DBLog>> GetLogs();

        Task AckLogs(int[] logNotificationIds);
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
    }
}