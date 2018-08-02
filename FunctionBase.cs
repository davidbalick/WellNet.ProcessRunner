using System.Collections.Generic;

namespace WellNet.ProcessRunner
{
    public class FunctionBase<Job>
    {
        protected FluentSchedulerJob CallingJob;
        protected Dictionary<string, string> _parameters;
        public string Context { get; set; } 

        public FunctionBase(FluentSchedulerJob fluentSchedulerJob, int setupJobFunctionId)
        {
            _parameters = StaticResources.GetParametersForForSetupJobFunction(CallingJob.EventJobId, setupJobFunctionId);
            Context = string.Format("Event {0}, Job {0}({1}), {2}", 
                CallingJob.EventJobId, CallingJob.JobName, CallingJob.SetupJobId, GetType().Name);
        }
    }
}
