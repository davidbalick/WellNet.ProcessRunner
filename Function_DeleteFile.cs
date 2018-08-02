using System.IO;

namespace WellNet.ProcessRunner
{
    public class Function_DeleteFile : FunctionBase<FluentSchedulerJob>, IFunction
    {
        public Function_DeleteFile(FluentSchedulerJob fluentSchedulerJob, int setupJobFunctionId) : base(fluentSchedulerJob, setupJobFunctionId)
        {
        }

        public void Execute()
        {
            var localFile = CallingJob.PropertyBag[JobPropertyBagKey.Filename].ToString();
            File.Delete(localFile);
        }
    }
}
