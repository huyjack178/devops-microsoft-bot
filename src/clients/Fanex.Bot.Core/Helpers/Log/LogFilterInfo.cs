namespace Fanex.Bot.Skynex.Helpers.Log
{
    internal static class LogFilterInfo
    {
        internal const string SessionInfo = "SESSION INFO";

        internal static readonly string[] SessionInfoKeys = new[] {
            "Username", "AccUserName", "CustRoleId", "custid","CustUname",
            "MemberID", "MemberUserName", "AgentID", "AgentUserName", "MasterID", "MasterUserName",
            "SuperID", "SusperUserName", "IsSyncCSCurrentCust", "IsInternal", "sitename"
        };
    }
}