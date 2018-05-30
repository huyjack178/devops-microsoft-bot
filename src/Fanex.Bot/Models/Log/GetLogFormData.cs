namespace Fanex.Bot.Models
{
    public class GetLogFormData
    {
        public string From { get; set; }

        public string To { get; set; }

        public string Severity { get; set; }

        public int Page { get; set; }

        public int Size { get; set; }

        public int ToGMT { get; set; }

        public int CategoryId { get; set; }

        public int MachineId { get; set; }

        public bool IsProduction { get; set; }
    }
}