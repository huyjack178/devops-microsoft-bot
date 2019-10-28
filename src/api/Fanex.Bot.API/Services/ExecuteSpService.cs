namespace Fanex.Bot.API.Services
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Bot.API.DbParams.Criterias;
    using Fanex.Bot.Core.ExecuteSP.Models;
    using Fanex.Data.Repository;
    using Newtonsoft.Json;

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
                GroupId = param.GroupId,
                Commands = param.Command
            };

            if (string.Equals(param.SpName, criteria.GetSettingKey(), StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var query = await dynamicRepository.FetchAsync<object>(criteria).ConfigureAwait(false);
                    result.IsSuccessful = true;
                    result.Message = JsonConvert.SerializeObject(query);
                }
                catch (Exception ex)
                {
                    result.IsSuccessful = false;
                    result.Message = ex.Message;
                }
            }
            else
            {
                result.Message = "The SP Name is incorrect.";
            }

            return result;
        }
    }
}