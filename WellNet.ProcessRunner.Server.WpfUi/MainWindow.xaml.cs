using System.Windows;

namespace WellNet.ProcessRunner.Server.WpfUi
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            tabServer.DataContext = new ViewModelServer();
            tabKindConfig.DataContext = new ViewModelKind();
        }
    }
}
