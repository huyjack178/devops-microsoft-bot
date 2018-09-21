namespace Fanex.Bot
{
    public static class MessageFormatSignal
    {
        public const string NewLine = "{{NewLine}}";
        public const string DoubleNewLine = "{{DoubleNewLine}}";
        public const string BeginBold = "{{BeginBold}}";
        public const string EndBold = "{{EndBold}}";
        public const string BreakLine = "{{BreakLine}}";
    }

    public static class Channel
    {
        public const string Skype = "skype";
        public const string Line = "line";
    }

    public static class MessageCommand
    {
        public const string UM = "um";
        public const string Start = "start";
        public const string Stop = "stop";
        public const string UM_AddPage = "addpage";
        public const string UM_Notify = "notify";
        public const string DBLOG = "dblog";
    }
}