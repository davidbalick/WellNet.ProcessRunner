using System.Collections.Generic;

namespace WellNet.ProcessRunner
{
    public class FunctionBase<Job>
    {
        protected FluentSchedulerJob _fluentSchedulerJob;
        protected Dictionary<string, string> _parameters;
        public string Context { get; set; } 

        public FunctionBase(FluentSchedulerJob fluentSchedulerJob, int setupJobFunctionId)
        {
            _parameters = StaticResources.GetParametersForForSetupJobFunction(_fluentSchedulerJob.EventJobId, setupJobFunctionId);
            Context = string.Format("Event {0}, Job {0}({1}), {2}", 
                _fluentSchedulerJob.EventJobId, _fluentSchedulerJob.JobName, _fluentSchedulerJob.SetupJobId, GetType().Name);
        }
    }
}
