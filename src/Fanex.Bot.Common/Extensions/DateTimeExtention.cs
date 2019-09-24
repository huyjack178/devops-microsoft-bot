namespace Fanex.Bot.Common.Extensions
{
    using System;

    public static class DateTimeExtention
    {
        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds);
        }

        public static DateTime GetUTCNow()
            => DateTime.UtcNow;

        public static DateTime ConvertFromSourceGMTToEndGMT(this DateTime date, int sourceGMT, int endGMT)
            => date.AddHours(-1 * sourceGMT).AddHours(endGMT);

        public static string GenerateGMTText(int GMT)
          => GMT < 0 ? GMT.ToString() : $"+{GMT}";
    }
}