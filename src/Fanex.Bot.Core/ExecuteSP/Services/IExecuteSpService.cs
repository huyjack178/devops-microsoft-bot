namespace Fanex.Bot.Core.ExecuteSP.Services
{
    using System.Threading.Tasks;
    using Fanex.Bot.Core.ExecuteSP.Models;

    public interface IExecuteSpService
    {
        Task<ExecuteSpResult> ExecuteSpWithParams(string message);
    }
}