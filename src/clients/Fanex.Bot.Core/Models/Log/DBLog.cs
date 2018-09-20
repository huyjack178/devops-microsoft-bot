namespace Fanex.Bot.Models.Log
{
    using System;

    public class DBLog
    {
        public int NotificationId { get; set; }

        public string ServerName { get; set; }

        public string Title { get; set; }

        public string MsgInfo { get; set; }

        public DateTime LogDate { get; set; }

        public string SkypeGroupId { get; set; }

        public bool IsSimple { get; set; }
    }
}