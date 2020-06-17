using Fanex.Data.Repository;

namespace Fanex.Bot.API.DbParams.Criterias
{
    public class GetNewDbLogCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "Get_NewDBLog";

        public override bool IsValid() => true;
    }
}