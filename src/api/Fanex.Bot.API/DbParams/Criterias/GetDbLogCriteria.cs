namespace Fanex.Bot.API.DbParams.Criterias
{
    using Fanex.Data.Repository;

    public class GetDbLogCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "Get_DBLog";

        public override bool IsValid() => true;
    }
}