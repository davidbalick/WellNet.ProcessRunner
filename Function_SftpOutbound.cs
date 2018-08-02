using System;
using System.IO;
using System.Linq;
using WellNet.Ftp;

namespace WellNet.ProcessRunner
{
    public class Function_SftpOutbound : FunctionBase<FluentSchedulerJob>, IFunction
    {
        public Function_SftpOutbound(FluentSchedulerJob fluentSchedulerJob, int setupJobFunctionId) : base(fluentSchedulerJob, setupJobFunctionId)
        {
        }

        public void Execute()
        {
            var folder = _parameters.ContainsKey("RemoteFolder") ? _parameters["RemoteFolder"] : "/";
            var localFile = CallingJob.PropertyBag[JobPropertyBagKey.Filename].ToString();

            var remoteFile = Path.Combine(folder, Path.GetFileName(localFile)).Replace("/",@"\");
            int numRetrys = 3;

            var vendorId = Convert.ToInt32(_parameters["Vendor"]);
            var transmissionSite = (new ProcessRunnerDcDataContext()).TransmissionSites.Single(ts => ts.VendorId == vendorId);

            var sftpProg = new SshNetSftp();
            ProcessRunnerException pre = null;
            try
            {
                sftpProg.DoWork(
                    SftpOperation.Upload,
                    transmissionSite.Site,
                    0,
                    transmissionSite.LoginName,
                    transmissionSite.Password,
                    null,
                    new[] { remoteFile },
                    new[] { localFile },
                    ref numRetrys
                    );
            } catch (Exception ex)
            {
                if (CallingJob.PropertyBag.ContainsKey(JobPropertyBagKey.ArchivedFilename))
                {
                    var archiveFile = CallingJob.PropertyBag[JobPropertyBagKey.ArchivedFilename].ToString();
                    StaticResources.MarkFileAsFailed(archiveFile);
                }
                pre = new ProcessRunnerException(Context, ex.Message);
            }
            File.Delete(localFile);
            if (pre != null)
                throw pre;
        }
    }
}
