namespace Fanex.Bot.Core.Bot.Models
{
    public class Result
    {
        public Result(int errorCode, string message = "")
        {
            ErrorCode = errorCode;
            Message = message;
        }

        public int ErrorCode { get; }

        public string Message { get; }

        public bool IsOk => ErrorCode == 0;

        public bool IsNotOk => !IsOk;

        public static Result CreateSuccessfulResult(string message = "")
            => new Result(0, message);

        public static Result CreateFailedResult(string message = "")
           => new Result(1, message);
    }
}