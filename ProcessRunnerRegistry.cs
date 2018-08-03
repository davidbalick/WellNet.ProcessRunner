using FluentScheduler;

namespace WellNet.ProcessRunner
{
    public class ProcessRunnerRegistry : Registry
    {
        public ProcessRunnerRegistry()
        {
            NonReentrantAsDefault();
        }
    }
}
