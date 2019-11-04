namespace Fanex.Bot.Core._Shared.Enumerations
{
    public class FunctionType : Enumeration
    {
        public const string BotFunctionName = "bot";
        public const string LogMSiteFunctionName = "log_msite";
        public const string LogDbFunctionName = "log_database";
        public const string LogSentryFunctionName = "log_sentry";
        public const string UnderMaintenanceFunctionName = "um";
        public const string GitLabFunctionName = "gitlab";
        public const string ZabbixFunctionName = "zabbix";
        public const string ExecuteSpFunctionName = "query";

        public static readonly FunctionType Bot = new FunctionType(1, BotFunctionName);

        public static readonly FunctionType LogMSite = new FunctionType(2, LogMSiteFunctionName);

        public static readonly FunctionType LogDb = new FunctionType(3, LogDbFunctionName);

        public static readonly FunctionType LogSentry = new FunctionType(4, LogSentryFunctionName);

        public static readonly FunctionType UnderMaintenance = new FunctionType(5, UnderMaintenanceFunctionName);

        public static readonly FunctionType GitLab = new FunctionType(6, GitLabFunctionName);

        public static readonly FunctionType Zabbix = new FunctionType(7, ZabbixFunctionName);

        public static readonly FunctionType ExecuteSP = new FunctionType(8, ExecuteSpFunctionName);

        public FunctionType()
        {
        }

        private FunctionType(byte value, string displayName)
            : base(value, displayName)
        {
        }
    }
}