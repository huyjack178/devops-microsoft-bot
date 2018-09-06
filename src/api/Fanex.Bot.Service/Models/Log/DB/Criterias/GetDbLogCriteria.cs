using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fanex.Data.Repository;

namespace Fanex.Bot.Service.Models.Log.DB.Criterias
{
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