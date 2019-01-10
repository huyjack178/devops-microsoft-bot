namespace Fanex.Bot
{
    public static class MessageFormatSignal
    {
        public const string NEWLINE = "{{NewLine}}";
        public const string DOUBLE_NEWLINE = "{{DoubleNewLine}}";
        public const string BOLD_START = "{{BeginBold}}";
        public const string BOLD_END = "{{EndBold}}";
        public const string DIVIDER = "{{BreakLine}}";
    }

    public static class Channel
    {
        public const string SKYPE = "skype";
        public const string LINE = "line";
    }

    public static class MessageCommand
    {
        public const string UM = "um";
        public const string START = "start";
        public const string STOP = "stop";
        public const string UM_ADD_PAGE = "addpage";
        public const string UM_NOTIFY = "notify";
        public const string UM_START_SCAN = "scanstart";
        public const string UM_STOP_SCAN = "scanstop";
        public const string DBLOG = "dblog";
        public const string ZABBIX = "zabbix";
        public const string ZABBIX_START_SCAN_SERVICE = "startscanservice";
        public const string ZABBIX_STOP_SCAN_SERVICE = "stopscanservice";
    }

    public static class HangfireJob
    {
        public const string ZABBIX_SCAN_SERVICE = "ZABBIX_SCAN_SERVICE";
        public const string UM_CHECK = "UM_CHECK";
        public const string WEBLOG_CHECK = "WEBLOG_CHECK";
    }
}