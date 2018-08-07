using System;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Linq;
using WellNet.Utils;

namespace WellNet.ProcessRunner.Server.WpfUi
{
    public class ViewModelKind : ViewModelBase
    {
        #region Fields
        #endregion Fields

        #region Propertys
        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand DiscardCommand { get; set; }
        public RelayCommand DeleteKfpCommand { get; set; }

        private ProcessRunnerDcDataContext _dc;
        private ProcessRunnerDcDataContext Dc
        {
            set
            {
                _dc = value;
                GetTables();
            }
            get { return _dc; }
        }
        private bool AreUncommittedChanges
        {
            get
            {
                var changeSet = Dc.GetChangeSet();
                return changeSet.Deletes.Count > 0 || changeSet.Inserts.Count > 0 || changeSet.Updates.Count > 0;
            }
        }

        private Table<Kind_Function> _kindFunctions;
        public Table<Kind_Function> KindFunctions
        {
            get { return _kindFunctions; }
            set
            {
                _kindFunctions = value;
                OnPropertyChanged("KindFunctions");
            }
        }

        private Kind_Function _selectedFunction;
        public Kind_Function SelectedFunction
        {
            get { return _selectedFunction; }
            set
            {
                _selectedFunction = value;
                RefreshKindFunctionParameters();
                OnPropertyChanged("SelectedFunction");
            }
        }

        private Table<Kind_Parameter> _kindParameters;
        public Table<Kind_Parameter> KindParameters
        {
            get { return _kindParameters; }
            set
            {
                _kindParameters = value;
                OnPropertyChanged("KindParameters");
            }
        }
        private ObservableCollection<Kind_FunctionParameterVm> _kindFunctionParameters;
        public ObservableCollection<Kind_FunctionParameterVm> KindFunctionParameters
        {
            get { return _kindFunctionParameters; }
            set
            {
                _kindFunctionParameters = value;
                OnPropertyChanged("KindFunctionParameters");
            }
        }

        private Kind_FunctionParameterVm _selectedKfp;
        public Kind_FunctionParameterVm SelectedKfp
        {
            get { return _selectedKfp; }
            set
            {
                _selectedKfp = value;
                OnPropertyChanged("SelectedKfp");
            }
        }


        #endregion Propertys

        public ViewModelKind()
        {
            RefreshCommand = new RelayCommand(RefreshAndDiscardExecMethod, RefreshCanExecMethod);
            SaveCommand = new RelayCommand(SaveExecMethod, SaveAndDiscardCanExecMethod);
            DiscardCommand = new RelayCommand(RefreshAndDiscardExecMethod, SaveAndDiscardCanExecMethod);
            DeleteKfpCommand = new RelayCommand(DeleteKfpExecMethod, DeleteKfpCanExecMethod);
            Refresh();
        }

        public void AddParameterToFunction(int kindParameterId)
        {
            var kfp = new Kind_FunctionParameter();
            kfp.Kind_FunctionId = SelectedFunction.Id;
            kfp.Kind_ParameterId = kindParameterId;
            KindFunctionParameters.Add(new Kind_FunctionParameterVm(kfp));
            Dc.Kind_FunctionParameters.InsertOnSubmit(kfp);
            OnPropertyChanged("KindFunctionParameters");
        }
        public void RemoveSelectedKfp()
        {
            if (SelectedKfp == null)
                return;
            Dc.Kind_FunctionParameters.DeleteOnSubmit(SelectedKfp.Kfp);
            KindFunctionParameters.Remove(SelectedKfp);
            SelectedKfp = null;
            OnPropertyChanged("KindFunctionParameters");
        }

        public void RefreshKindFunctionParameters()
        {
            KindFunctionParameters = new ObservableCollection<Kind_FunctionParameterVm>();
            if (SelectedFunction == null)
                return;
            foreach (var kfp in Dc.Kind_FunctionParameters.Where(kfp => kfp.Kind_FunctionId == SelectedFunction.Id))
                KindFunctionParameters.Add(new Kind_FunctionParameterVm(kfp));
        }

        private void GetTables()
        {
            KindFunctions = Dc.Kind_Functions;
            KindParameters = Dc.Kind_Parameters;
        }

        private void Refresh()
        {
            Dc = new ProcessRunnerDcDataContext();
            Status = string.Empty;
        }

        #region CommandMethods
        private bool RefreshCanExecMethod(object arg) { return !AreUncommittedChanges; }
        private void RefreshAndDiscardExecMethod(object obj) { Refresh(); }
        private bool SaveAndDiscardCanExecMethod(object arg) { return AreUncommittedChanges; }
        private void SaveExecMethod(object obj)
        {
            Dc.SubmitChanges();
            Refresh();
            Status = "Changes are saved";
        }
        private bool DeleteKfpCanExecMethod(object arg)
        {
            return true;
        }
        private void DeleteKfpExecMethod(object obj)
        {
            RemoveSelectedKfp();
        }
        #endregion CommandMethods

    }
    public class Kind_FunctionParameterVm : Kind_FunctionParameter
    {
        public Kind_FunctionParameter Kfp;

        public string ParameterName
        {
            get { return LookupParameterName(); }
        }
        public Kind_FunctionParameterVm(Kind_FunctionParameter kfp)
        {
            Kfp = kfp;
            Id = kfp.Id;
            Kind_FunctionId = kfp.Kind_FunctionId;
            Kind_ParameterId = kfp.Kind_ParameterId;
        }

        private string LookupParameterName()
        {
            if (Kfp.Kind_Parameter != null)
                return Kfp.Kind_Parameter.Name;
            return (new ProcessRunnerDcDataContext()).Kind_Parameters.Single(kp => kp.Id == Kind_ParameterId).Name;
        }
    }
}
