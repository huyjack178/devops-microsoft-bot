namespace Fanex.Bot.API.Models.Log.DB.Criterias
{
    using System;
    using Fanex.Data.Repository;

    public class GetDbLogCriteria : CriteriaBase
    {
        public override string GetSettingKey()
            => "DBA_Skype_Notify";

        public override bool IsValid()
        {
            throw new NotImplementedException();
        }
    }
}