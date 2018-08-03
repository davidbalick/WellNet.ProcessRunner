using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using WellNet.Utils;
using FluentScheduler;

namespace WellNet.ProcessRunner.Server.WpfUi
{

    public class ViewModelServer : ViewModelBase
    {
        private BackgroundWorker _serverBg;
        //private ServiceLoop _serviceLoop;
        public RelayCommand StartCommand;
        public RelayCommand StopCommand;
        private int _waitMilliseconds;

        public ViewModelServer()
        {
            _serverBg = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _serverBg.DoWork += ServerBg_DoWork;
            _serverBg.RunWorkerCompleted += ServerBg_RunWorkerCompleted;
            _serverBg.ProgressChanged += ServerBg_ProgressChanged;
            //_serviceLoop = new ServiceLoop();
            StartCommand = new RelayCommand(StartExecMethod, StartCanExecMethod);
            StopCommand = new RelayCommand(StopExecMethod, StopCanExecMethod);
            _waitMilliseconds = StaticResources.GetPollWaitMilliseconds();
            JobManager.Initialize(new ProcessRunnerRegistry());
        }

        private bool StartCanExecMethod(object arg)
        {
            return !_serverBg.IsBusy;
        }
        private void StartExecMethod(object obj)
        {
            _serverBg.RunWorkerAsync();
        }
        private bool StopCanExecMethod(object arg)
        {
            return _serverBg.IsBusy;
        }
        private void StopExecMethod(object obj)
        {
            _serverBg.CancelAsync();
        }

        private void ServerBg_DoWork(object sender, DoWorkEventArgs e)
        {
            StaticResources.LogMessage(null, EventMessageSeverity.Documentation, null, "Service Started", _serverBg);
            while (_serverBg.CancellationPending)
                try
                {
                    PerformService();
                }
                catch (Exception ex)
                {
                    var context = (ex is ProcessRunnerException) ? ((ProcessRunnerException)ex).Context : "PerformService failed";
                    StaticResources.LogMessage(null, EventMessageSeverity.Fatal, context, ex.Message, _serverBg);
                    break;
                }
            StaticResources.LogMessage(null, EventMessageSeverity.Fatal, null, "Service Stopped", _serverBg);
        }

        private void PerformService()
        {
            if (_serverBg.CancellationPending)
                return;
            Thread.Sleep(_waitMilliseconds);
            var dc = new ProcessRunnerDcDataContext();
            foreach (var eventJob in dc.Event_Jobs.Where(ej => ej.Status == null))
            {
                var fluentSchedulerJob = new FluentSchedulerJob(eventJob.Id, _serverBg);
                JobManager.AddJob(fluentSchedulerJob, s => s.ToRunNow());
            }

        }

        private void ServerBg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Log(e.UserState.ToString());
        }

        private void ServerBg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                Log(e.Error.Message);
        }

        private void Log(string message)
        {
            Status += string.Format("\n{0}: {1}", DateTime.Now, message);
        }

    }
}
