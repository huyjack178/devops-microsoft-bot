using Fanex.Data.Repository;

namespace Fanex.Bot.API.DbParams.Commands
{
    public class AckNewDbLogCommand : NonQueryCommand
    {
        public AckNewDbLogCommand(int[] notiList)
        {
            NotiList = string.Join(",", notiList);
        }

        public string NotiList { get; }

        public override string GetSettingKey() => "Ack_NewDBLog";

        public override bool IsValid() => !string.IsNullOrEmpty(NotiList);
    }
}