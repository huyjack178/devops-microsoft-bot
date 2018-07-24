namespace Fanex.Bot.Service.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Fanex.Bot.Service.Models.Log;
    using Fanex.Data.Repository;

    /// <summary>
    /// Log service.
    /// </summary>
    public class LogService
    {
        private readonly IDynamicRepository _dynamicRepository;

        public LogService(IDynamicRepository dynamicRepository)
        {
            _dynamicRepository = dynamicRepository;
        }

        public async Task<IEnumerable<Log>> GetLogsAsync(GetLogCriteria criteria)
        {
            var logs = await _dynamicRepository.FetchAsync<Log>(criteria);

            if (logs.Any())
            {
                logs = logs.Select(log =>
                {
                    log.FormattedMessage = HttpUtility.HtmlEncode(log.FormattedMessage);
                    log.FormattedMessage = ReplaceTimestampFromLogMessage(log.FormattedMessage, criteria.ToGMT);

                    return log;
                });
            }

            return logs;
        }

        private string ReplaceTimestampFromLogMessage(string message, int GMT)
        {
            var messageIndex = message.IndexOf("\r", StringComparison.InvariantCulture) - 1;
            var timestampLength = messageIndex - message.IndexOf(":", StringComparison.InvariantCulture);
            var timestamp = message.Substring(10, timestampLength);
            var offsetSign = GMT > 0 ? "+" : string.Empty;

            message = message.Replace(
                timestamp,
                $" {Convert.ToDateTime(timestamp).AddHours(GMT).ToString()} (GMT{offsetSign}{GMT})");

            return message;
        }
    }
}