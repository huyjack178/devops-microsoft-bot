namespace Fanex.Bot.Service.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Fanex.Bot.Service.Models.Log;
    using Fanex.Data.Repository;

    public interface ILogService
    {
        Task<IEnumerable<Log>> GetLogsAsync(GetLogCriteria criteria);
    }

    public class LogService : ILogService
    {
        private readonly IDynamicRepository _dynamicRepository;

        public LogService(IDynamicRepository dynamicRepository)
        {
            _dynamicRepository = dynamicRepository;
        }

#pragma warning disable S3216 // "ConfigureAwait(false)" should be used

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

#pragma warning restore S3216 // "ConfigureAwait(false)" should be used

        private static string ReplaceTimestampFromLogMessage(string message, int GMT)
        {
            var messageIndex = message.IndexOf("\r", StringComparison.InvariantCulture) - 1;
            var timestampLength = messageIndex - message.IndexOf(":", StringComparison.InvariantCulture);
            var timestamp = message.Substring(10, timestampLength);
            var offsetSign = GMT > 0 ? "+" : string.Empty;

            return message.Replace(
                timestamp,
                $" {Convert.ToDateTime(timestamp).AddHours(GMT).ToString(CultureInfo.InvariantCulture)} (GMT{offsetSign}{GMT})");
        }
    }
}