using Fanex.Bot.Core._Shared.Enumerations;

namespace Fanex.Bot.Core.GitLab.Models.JobEvents
{
    public class JobStatus : Enumeration
    {
        public const string CreatedType = "created";
        public const string RunningType = "running";
        public const string SuccessType = "success";
        public const string FailedType = "failed";
        public const string CanceledType = "canceled";
        public const string ManualType = "manual";

        public static readonly JobStatus Created = new JobStatus(1, CreatedType);

        public static readonly JobStatus Running = new JobStatus(2, RunningType);

        public static readonly JobStatus Success = new JobStatus(3, SuccessType);

        public static readonly JobStatus Failed = new JobStatus(4, FailedType);

        public static readonly JobStatus Canceled = new JobStatus(5, CanceledType);

        public static readonly JobStatus Manual = new JobStatus(6, ManualType);

        public JobStatus()
        {
        }

        private JobStatus(byte value, string displayName)
            : base(value, displayName)
        {
        }

        public bool IsCreated => this == Created;

        public bool IsRunning => this == Running;

        public bool IsSuccess => this == Success;

        public bool IsFailed => this == Failed;
    }
}