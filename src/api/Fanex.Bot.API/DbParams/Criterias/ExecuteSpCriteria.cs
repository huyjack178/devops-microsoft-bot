namespace Fanex.Bot.API.DbParams.Criterias
{
    using Fanex.Data.Repository;

    public class ExecuteSpCriteria : CriteriaBase
    {
        public string GroupId { get; set; }

        public string Commands { get; set; }

        public override string GetSettingKey() => "Skype_BotQuery";

        public override bool IsValid() => true;
    }
}