namespace Fanex.Bot.API.DbParams.Commands
{
    using Fanex.Data.Repository;

    public class AckDbLogCommand : NonQueryCommand
    {
        public AckDbLogCommand(int[] notiList)
        {
            NotiList = string.Join(",", notiList);
        }

        public string NotiList { get; }

        public override string GetSettingKey() => "Ack_DBLog";

        public override bool IsValid() => !string.IsNullOrEmpty(NotiList);
    }
}