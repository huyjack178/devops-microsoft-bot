namespace Fanex.Bot.API.DbParams.Criterias
{
    using Fanex.Data.Repository;

    public class GetDbLogCriteria : CriteriaBase
    {
        public GetDbLogCriteria(int minute = 5)
        {
            Minutes = minute;
        }

        public int Minutes { get; }

        public override string GetSettingKey()
            => "Get_DBLog";

        public override bool IsValid()
            => Minutes > 0;
    }
}