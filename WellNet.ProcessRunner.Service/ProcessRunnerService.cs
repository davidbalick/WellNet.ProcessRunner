using System.ServiceProcess;
using System.Threading;

namespace WellNet.ProcessRunner.Service
{
    public partial class ProcessRunnerService : ServiceBase
    {
        private ServiceLoop _serviceLoop;

        public ProcessRunnerService()
        {
            InitializeComponent();
            _serviceLoop = new ServiceLoop();
        }

        protected override void OnStart(string[] args)
        {
            StaticResources.LogMessage(null, StaticResources.Severity.Documentation, null, "Service Started", null);
            _serviceLoop.Start(null);
        }

        protected override void OnStop()
        {
            _serviceLoop.StopRequested = true;
            while (!_serviceLoop.Stopped)
                Thread.Sleep(5000);
            StaticResources.LogMessage(null, StaticResources.Severity.Fatal, null, "Service Stopped", null);
            base.OnStop();
        }
    }
}
