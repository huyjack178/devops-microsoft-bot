namespace Fanex.Bot.Service.Models.Log
{
    using System;

    public class DBLog
    {
        public string NotificationId { get; set; }

        public string ServerName { get; set; }

        public string Title { get; set; }

        public string MsgInfo { get; set; }

        public DateTime LogDate { get; set; }

        public string SkypeGroupId { get; set; }

        public bool IsSimple { get; set; }
    }
}