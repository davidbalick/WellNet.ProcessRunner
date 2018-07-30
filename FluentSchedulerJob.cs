using FluentScheduler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace WellNet.ProcessRunner
{
    public class FluentSchedulerJob : IJob
    {
        public enum PropertyBagKey
        {
            Filename
        };

        public readonly int EventJobId;
        public readonly int SetupJobId;
        public readonly string JobName;
        public BackgroundWorker BgWorker;
        public Dictionary<PropertyBagKey, object> PropertyBag { get; set; }

        public FluentSchedulerJob(int eventJobId, BackgroundWorker bgWorker)
        {
            EventJobId = eventJobId;
            var dc = new ProcessRunnerDcDataContext();
            var eventJob = dc.Event_Jobs.Single(ej => ej.Id == eventJobId);
            SetupJobId = eventJob.Setup_JobId;
            JobName = dc.Setup_Jobs.Single(sj => sj.Id == SetupJobId).Name;

            BgWorker = bgWorker;
            PropertyBag = new Dictionary<PropertyBagKey, object>();
        }


        public void Execute()
        {
            var dc = new ProcessRunnerDcDataContext();
            var status = StaticResources.KindStatus.Working;
            StaticResources.UpdateEventJobStatusAndRunWhen(EventJobId, status);
            foreach (var sjf in dc.Setup_JobFunctions.Where(sjf => sjf.Setup_JobId == SetupJobId && sjf.IsDisabled != true).OrderBy(sjf => sjf.Sequence))
            {
                var funcName = string.Format("Function_{0}", dc.Kind_Functions.Single(kf => kf.Id == sjf.Kind_FunctionId).Name);
                var func = (IFunction) Activator.CreateInstance(Assembly.GetExecutingAssembly().GetName().Name, funcName, new[] { (object)this, sjf.Id});
                try
                {
                    func.Execute();
                } catch (Exception ex)
                {
                    StaticResources.UpdateEventJobStatusAndRunWhen(EventJobId, StaticResources.KindStatus.Failed);
                    if (ex is ProcessRunnerException)
                        throw ex;
                    throw new ProcessRunnerException(func.Context, string.Format("Unhandled exception: {0}", ex.Message));
                }
                if (FatalErrorWasReported())
                {
                    status = StaticResources.KindStatus.Failed;
                    break;
                }
            }
            status = status == StaticResources.KindStatus.Working ? StaticResources.KindStatus.Completed : status;
            StaticResources.UpdateEventJobStatusAndRunWhen(EventJobId, status);
        }

        private bool FatalErrorWasReported()
        {
            var fatal = (int)StaticResources.Severity.Fatal;
            return (new ProcessRunnerDcDataContext()).Event_Messages.Any(em => em.Event_JobId == EventJobId && em.Severity == fatal);
        }
    }
}
