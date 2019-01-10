namespace Fanex.Bot.Enums
{
    public enum ZabbixServiceStatus
    {
        Running = 0,
        Paused = 1,
        StartPending = 2,
        PausePending = 3,
        ContinuePending = 4,
        StopPending = 5,
        Stopped = 6,
        Unknown = 7,
        NoSuchService = 255
    }
}