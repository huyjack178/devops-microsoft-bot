namespace Fanex.Bot.API.DbParams.Criterias
{
    using Fanex.Data.Repository;

    public class GetDbLogCriteria : CriteriaBase
    {
        public GetDbLogCriteria(int minute = 5)
        {
            Minute = minute;
        }

        public int Minute { get; }

        public override string GetSettingKey()
            => "Get_DBLog";

        public override bool IsValid()
            => Minute > 0;
    }
}