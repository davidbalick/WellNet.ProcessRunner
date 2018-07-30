using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace WellNet.ProcessRunner.Server.WpfUi
{
    public partial class MainWindow : Window
    {
        private BackgroundWorker _serverBg;
        private ServiceLoop _serviceLoop;

        public MainWindow()
        {
            InitializeComponent();
            _serverBg = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _serverBg.DoWork += ServerBg_DoWork;
            _serverBg.RunWorkerCompleted += ServerBg_RunWorkerCompleted;
            _serverBg.ProgressChanged += ServerBg_ProgressChanged;
            _serviceLoop = new ServiceLoop();
            BtnStart.Click += BtnStart_Click;
            BtnStop.Click += BtnStop_Click;
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _serviceLoop.StopRequested = true;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            _serverBg.RunWorkerAsync();
        }

        private void ServerBg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                Log(e.Error.Message);
        }

        private void ServerBg_DoWork(object sender, DoWorkEventArgs e)
        {
            _serviceLoop.Start(_serverBg);
        }

        private void ServerBg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Log(e.UserState.ToString());
        }

        private void Log(string message)
        {
            TbStatus.Text += string.Format("\n{0}: {1}", DateTime.Now, message);
        }
    }
}
