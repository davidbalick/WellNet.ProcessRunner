using System;
using System.ComponentModel;
using System.Threading;

namespace WellNet.ProcessRunner
{
    //Not currently used
    public class ServiceLoop
    {
        private int _waitMilliseconds;
        public bool StopRequested;
        public bool Stopped;
        private BackgroundWorker _bgWorker;

        public void Start(BackgroundWorker bgWorker)
        {
            if (bgWorker != null)
                _bgWorker = bgWorker;
            _waitMilliseconds = StaticResources.GetPollWaitMilliseconds();
            StaticResources.LogMessage(null, EventMessageSeverity.Documentation, null, "Service Started", _bgWorker);
            while (!StopRequested)
                try
                {
                    PerformService();
                } catch (Exception ex)
                {
                    var context = (ex is ProcessRunnerException) ? ((ProcessRunnerException)ex).Context : "PerformService failed";
                    StaticResources.LogMessage(null, EventMessageSeverity.Fatal, context, ex.Message, _bgWorker);
                    break;
                }
            Stopped = true;
            StaticResources.LogMessage(null, EventMessageSeverity.Fatal, null, "Service Stopped", _bgWorker);
        }

        private void PerformService()
        {
            if (_bgWorker != null)
                _bgWorker.ReportProgress(0, "Tick");

            if (StopRequested) return;
            Thread.Sleep(_waitMilliseconds);
            //await PutTaskDelay();
        }

        //public async Task PutTaskDelay()
        //{
        //    await Task.Delay(_waitMilliseconds);
        //}
    }
}
