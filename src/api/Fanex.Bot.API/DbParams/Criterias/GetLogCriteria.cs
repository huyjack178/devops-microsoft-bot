namespace Fanex.Bot.API.DbParams.Criterias
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fanex.Data.Repository;

    public class GetLogCriteria : CriteriaBase
    {
        public int? CategoryId { get; set; }

        public int? MachineId { get; set; }

        public string Severity { get; set; }

        public bool? IsProduction { get; set; }

        public bool? IsMonitored { get; set; }

        [DataType(DataType.Date)]
        public DateTime From { get; set; }

        [DataType(DataType.Date)]
        public DateTime To { get; set; }

        public int Page { get; set; }

        public int Size { get; set; }

        public int TotalRows { get; set; }

        public int ToGMT { get; set; }

        public override string GetSettingKey()
            => "Logging_Sel_Padding";

        public override bool IsValid()
            => CategoryId != null;
    }
}