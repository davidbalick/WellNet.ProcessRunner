using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace WellNet.ProcessRunner.Server.WpfUi
{
    public partial class MainWindow : Window
    {
        private ViewModelKind _viewModelKind;
        private ViewModelSetup _viewModelSetup;
        public delegate Point GetPosition(IInputElement element);
        //private int _rowIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            tabServer.DataContext = new ViewModelServer();
            _viewModelKind = new ViewModelKind();
            tabKindConfig.DataContext = _viewModelKind;
            _viewModelSetup = new ViewModelSetup();
            tabSetupConfig.DataContext = _viewModelSetup;

            LbxKindFunctionParameters.Drop += LbxKindFunctionParameters_Drop;
            LbxKindFunctionParameters.DragLeave += LbxKindFunctionParameters_DragLeave;
            DgrKindParameters.PreviewMouseLeftButtonDown += DgrKindParameters_PreviewMouseLeftButtonDown;
            LbxKindFunctionParameters.PreviewMouseLeftButtonDown += LbxKindFunctionParameters_PreviewMouseLeftButtonDown;
            BtnAddSetupParameter.Click += BtnAddEditSetupParameter_Click;
            BtnEditSetupParameter.Click += BtnAddEditSetupParameter_Click;

        }

        private void BtnAddEditSetupParameter_Click(object sender, RoutedEventArgs e)
        {
            _viewModelSetup.CrudAction = ((Button)sender).Name.Contains("Edit") ? ViewModelSetup.CrudActions.Editing : ViewModelSetup.CrudActions.Creating;
            var setupParmWindow = new SetupParameterWindow();
            setupParmWindow.Title = string.Format("{0} a Setup Parameter", _viewModelSetup.CrudAction);
            setupParmWindow.DataContext = _viewModelSetup;
            setupParmWindow.Owner = this;
            _viewModelSetup.CloseAction = new Action(() => setupParmWindow.Close());
            setupParmWindow.ShowDialog();
        }

        private void LbxKindFunctionParameters_Drop(object sender, DragEventArgs e)
        {
            if (_viewModelKind.SelectedFunction == null)
                return;
            var o = DgrKindParameters.SelectedItem;
            if (o == null)
                return;
            var kindParameter = o as Kind_Parameter;
            if (kindParameter == null)
                return;
            _viewModelKind.AddParameterToFunction(kindParameter.Id);
        }

        private void LbxKindFunctionParameters_DragLeave(object sender, DragEventArgs e)
        {
            var item = LbxKindFunctionParameters.SelectedItem;
            if (item == null)
                return;
            var kfpVm = item as Kind_FunctionParameterVm;
            if (kfpVm == null)
                return;
            MessageBox.Show(kfpVm.ParameterName);
        }

        private void DgrKindParameters_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var rowIndex = GetCurrentRowIndex(e.GetPosition);
            if (rowIndex < 0)
                return;
            DgrKindParameters.SelectedIndex = rowIndex;
            var selectedParam = DgrKindParameters.Items[rowIndex] as Kind_Parameter;
            if (selectedParam == null)
                return;
            DragDropEffects dragdropeffects = DragDropEffects.Move;
            if (DragDrop.DoDragDrop(DgrKindParameters, selectedParam, dragdropeffects) != DragDropEffects.None)
                DgrKindParameters.SelectedItem = selectedParam;
        }

        private void LbxKindFunctionParameters_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = LbxKindFunctionParameters.SelectedItem;
            if (item == null)
                return;
            DragDrop.DoDragDrop(LbxKindFunctionParameters, item, DragDropEffects.Move);
        }

        private bool GetMouseTargetRow(Visual theTarget, GetPosition position)
        {
            Rect rect = VisualTreeHelper.GetDescendantBounds(theTarget);
            Point point = position((IInputElement)theTarget);
            return rect.Contains(point);
        }

        private DataGridRow GetRowItem(int index)
        {
            if (DgrKindParameters.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;
            return DgrKindParameters.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
        }

        private int GetCurrentRowIndex(GetPosition pos)
        {
            int curIndex = -1;
            for (int i = 0; i < DgrKindParameters.Items.Count; i++)
            {
                DataGridRow itm = GetRowItem(i);
                if (GetMouseTargetRow(itm, pos))
                {
                    curIndex = i;
                    break;
                }
            }
            return curIndex;
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var inputElement = Mouse.DirectlyOver;
            e.Handled = !(inputElement is TextBlock);
        }

    }
}
