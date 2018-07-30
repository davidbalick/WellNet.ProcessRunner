using System;
using System.IO;
using System.Linq;
using WellNet.Ftp;

namespace WellNet.ProcessRunner
{
    public class Function_SftpOutboundAndArchive : FunctionBase<FluentSchedulerJob>, IFunction
    {
        public Function_SftpOutboundAndArchive(FluentSchedulerJob fluentSchedulerJob, int setupJobFunctionId) : base(fluentSchedulerJob, setupJobFunctionId)
        {
        }

        public void Execute()
        {
            var folder = _parameters.ContainsKey("RemoteFolder") ? _parameters["RemoteFolder"] : "/";
            var localFile = _fluentSchedulerJob.PropertyBag[FluentSchedulerJob.PropertyBagKey.Filename].ToString();

            var remoteFile = Path.Combine(folder, Path.GetFileName(localFile)).Replace("/",@"\");
            int numRetrys = 3;

            var vendorId = Convert.ToInt32(_parameters["Vendor"]);
            var transmissionSite = (new ProcessRunnerDcDataContext()).TransmissionSites.Single(ts => ts.VendorId == vendorId);

            var archiveFilename = StaticResources.ArchiveFile(vendorId, SftpDirection.Outbound, localFile);

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
                StaticResources.MarkFileAsFailed(archiveFilename);
                pre = new ProcessRunnerException(Context, ex.Message);
            }
            File.Delete(localFile);
            if (pre != null)
                throw pre;
        }
    }
}
