using System.Threading.Tasks;
using Hangfire;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public abstract class NoConcurrencyNoRetryJob
    {
        [DisableConcurrentExecution(timeoutInSeconds: 1000)]
        [AutomaticRetry(Attempts = 0)]
        public Task ExecuteJobAsync()
        {
            return ExecuteAsync();
        }

        protected abstract Task ExecuteAsync();
    }
}