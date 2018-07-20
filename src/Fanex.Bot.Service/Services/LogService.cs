//namespace Fanex.Bot.Service.Services
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;

//    /// <summary>
//    /// Log service.
//    /// </summary>
//    public class LogService
//    {
//        private readonly LogRepository _logRepository = new LogRepository();

//        private int? _categoryId;

//        private int? _machineId;

//        private bool? _isProduction;

//        private bool? _isMonitored;

//        private string _severity;

//        private DateTime _dateFrom;

//        private DateTime _dateTo;

//        private int _page;

//        private int _size;

//        private int _showLogTimeZone;

//        private string searchText;

//        private LogService(int showLogTimeZone = 0)
//        {
//            _categoryId = 0;
//            _severity = "All";
//            _machineId = 0;
//            _dateFrom = DateTime.Now;
//            _dateTo = DateTime.Now.AddDays(-1);
//            _page = 0;
//            _size = 100;
//            _showLogTimeZone = showLogTimeZone;
//            searchText = string.Empty;
//            removedLogCategories = string.Empty;
//        }

//        /// <summary>
//        /// Create log service.
//        /// </summary>
//        /// <param name="showLogTimeZone">Show log time zone.</param>
//        /// <returns>Log service object.</returns>
//        public static LogService Create(int showLogTimeZone = 0)
//        {
//            return new LogService(showLogTimeZone);
//        }

//        /// <summary>
//        /// By categoryId.
//        /// </summary>
//        /// <param name="categoryId">Category ID.</param>
//        /// <returns>Log service object.</returns>
//        public LogService With(int categoryId)
//        {
//            this._categoryId = categoryId;

//            return this;
//        }

//        /// <summary>
//        /// By categoryId and Severity.
//        /// </summary>
//        /// <param name="categoryId">Category ID.</param>
//        /// <param name="severity">Severity level.</param>
//        /// <returns>Log service object.</returns>
//        public LogService With(int? categoryId, string severity)
//        {
//            this._categoryId = categoryId;
//            this._severity = severity;

//            return this;
//        }

//        /// <summary>
//        /// By CategoryId, MachineId and Severity.
//        /// </summary>
//        /// <param name="categoryId">Category ID.</param>
//        /// <param name="machineId">Machine ID.</param>
//        /// <param name="isProduction">Production or not.</param>
//        /// <param name="isMonitored">Monitored or not.</param>
//        /// <param name="severity">Severity level.</param>
//        /// <returns>Log service object.</returns>
//        public LogService With(int? categoryId, int? machineId, bool? isProduction, bool? isMonitored, string severity, int timeZone, string searchText)
//        {
//            this._categoryId = categoryId;
//            this._machineId = machineId;
//            this._severity = severity;
//            this._isProduction = isProduction;
//            this._isMonitored = isMonitored;
//            this._showLogTimeZone = timeZone;
//            this.searchText = searchText;

//            return this;
//        }

//        /// <summary>
//        /// Update from date.
//        /// </summary>
//        /// <param name="fromDate">From date.</param>
//        /// <returns>Log service object.</returns>
//        public LogService From(DateTime fromDate)
//        {
//            this._dateFrom = fromDate;

//            return this;
//        }

//        /// <summary>
//        /// Update to date.
//        /// </summary>
//        /// <param name="toDate">To date value.</param>
//        /// <returns>Log service object.</returns>
//        public LogService To(DateTime toDate)
//        {
//            this._dateTo = toDate;

//            return this;
//        }

//        /// <summary>
//        /// Pagination items.
//        /// </summary>
//        /// <param name="page">Page index.</param>
//        /// <param name="size">Page size.</param>
//        /// <returns>Log service object.</returns>
//        public LogService WithPagination(int page, int size)
//        {
//            this._page = page;
//            this._size = size;

//            return this;
//        }

//        /// <summary>
//        /// Get message detail.
//        /// </summary>
//        /// <param name="logId">Log object ID.</param>
//        /// <returns>Message string.</returns>
//        public Log GetLog(long logId)
//        {
//            var log = _logRepository.GetLog(logId);

//            return log;
//        }

//        /// <summary>
//        /// Get list logs.
//        /// </summary>
//        /// <param name="totalRows">Number of rows.</param>
//        /// <returns>List of log.</returns>
//        public IEnumerable<Log> GetLogs(ref int totalRows)
//        {
//            var criteria = new GetLogCriteria
//            {
//                CategoryId = this._categoryId != null ? this._categoryId : 0,
//                From = this._dateFrom,
//                To = this._dateTo,
//                Page = this._page,
//                Size = this._size,
//                Severity = this._severity,
//                MachineId = this._machineId,
//                IsProduction = this._isProduction,
//                IsMonitored = this._isMonitored,
//                ToGMT = _showLogTimeZone,
//                SearchText = searchText,
//            };

//            var list = _logRepository.GetLog(ref criteria);

//            totalRows = criteria.TotalRows;

//            return list;
//        }
//    }
//}