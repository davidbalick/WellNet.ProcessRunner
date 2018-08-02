using System;
using System.Linq;

namespace WellNet.ProcessRunner
{
    public class Function_Archive : FunctionBase<FluentSchedulerJob>, IFunction
    {
        public Function_Archive(FluentSchedulerJob fluentSchedulerJob, int setupJobFunctionId) : base(fluentSchedulerJob, setupJobFunctionId)
        {
        }

        public void Execute()
        {
            var vendorId = Convert.ToInt32(_parameters["Vendor"]);
            var localFile = CallingJob.PropertyBag[JobPropertyBagKey.Filename].ToString();
            if (!(new[] {SetupJobDirection.Inbound, SetupJobDirection.Outbound}).Contains(CallingJob.JobDirection))
                throw new ProcessRunnerException(Context, "Bad Job Direction");
            CallingJob.PropertyBag[JobPropertyBagKey.ArchivedFilename] =
                StaticResources.ArchiveFile(vendorId, CallingJob.JobDirection, localFile);
        }
    }
}
