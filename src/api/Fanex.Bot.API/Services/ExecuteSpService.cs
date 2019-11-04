namespace Fanex.Bot.API.Services
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.API.DbParams.Criterias;
    using Fanex.Bot.Core.ExecuteSP.Models;
    using Fanex.Data.Repository;

    public interface IExecuteSpService
    {
        Task<ExecuteSpResult> ExecuteSP(ExecuteSpParam param);
    }

    public class ExecuteSpService : IExecuteSpService
    {
        private readonly IDynamicRepository dynamicRepository;

        public ExecuteSpService(IDynamicRepository dynamicRepository)
        {
            this.dynamicRepository = dynamicRepository;
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
                var query = await dynamicRepository.GetAsync<string>(criteria).ConfigureAwait(false);
                result.IsSuccessful = true;
                result.Message = query;
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}