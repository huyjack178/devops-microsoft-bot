namespace Fanex.Bot.API.Services
{
    using Fanex.Bot.API.DbParams.Criterias;
    using Fanex.Bot.Core.ExecuteSP.Models;
    using Fanex.Data.Repository;
    using Fanex.Logging;
    using Newtonsoft.Json;
    using System;
    using System.Threading.Tasks;

    public interface IExecuteSpService
    {
        Task<ExecuteSpResult> ExecuteSP(ExecuteSpParam param);
    }

    public class ExecuteSpService : IExecuteSpService
    {
        private readonly IDynamicRepository dynamicRepository;
        private readonly ILogger logger;

        public ExecuteSpService(IDynamicRepository dynamicRepository, ILogger logger)
        {
            this.dynamicRepository = dynamicRepository;
            this.logger = logger;
        }

        public async Task<ExecuteSpResult> ExecuteSP(ExecuteSpParam param)
        {
            var result = new ExecuteSpResult();
            var criteria = new ExecuteSpCriteria
            {
                ConversationId = param.ConversationId,
                Commands = param.Command
            };

            try
            {
                logger.Info($"dbc {JsonConvert.SerializeObject(param)}");

                var query = await dynamicRepository.GetAsync<string>(criteria).ConfigureAwait(false);
                result.IsSuccessful = true;
                result.Message = query;
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.Message = ex.Message;
                logger.Error(ex.Message, ex);
            }

            return result;
        }
    }
}