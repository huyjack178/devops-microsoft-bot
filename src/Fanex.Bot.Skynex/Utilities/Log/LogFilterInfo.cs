using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Bot.Skynex.Utilities.Log
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