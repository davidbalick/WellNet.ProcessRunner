using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WellNet.Utils;

namespace WellNet.ProcessRunner
{
    public class ViewModelSetup : ViewModelBase
    {
        public enum CrudActions
        {
            Creating,
            Editing,
            Deleting
        }

        #region Properties
        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand DiscardCommand { get; set; }
        public RelayCommand SaveSetupParameterCommand { get; set; }
        public RelayCommand DiscardSetupParameterCommand { get; set; }
        public Action CloseAction { get; set; }

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
        private Table<Setup_Parameter> _setupParameters;
        public Table<Setup_Parameter> SetupParameters
        {
            get { return _setupParameters; }
            set
            {
                _setupParameters = value;
                OnPropertyChanged("SetupParameters");
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
        private Setup_Parameter _selectedSetupParameter;
        public Setup_Parameter SelectedSetupParameter
        {
            get { return _selectedSetupParameter; }
            set
            {
                _selectedSetupParameter = value;
                if (SelectedSetupParameter == null)
                    SelectedKindParameter = null;
                else
                    SelectedKindParameter = KindParameters.FirstOrDefault(kp => kp.Id == SelectedSetupParameter.Kind_ParameterId);
                OnPropertyChanged("SelectedSetupParameter");
            }
        }
        private Kind_Parameter _selectedKindParameter;
        public Kind_Parameter SelectedKindParameter
        {
            get { return _selectedKindParameter; }
            set
            {
                _selectedKindParameter = value;
                OnPropertyChanged("SelectedKindParameter");
            }
        }
        public CrudActions _crudAction;
        public CrudActions CrudAction
        {
            get { return _crudAction; }
            set
            {
                _crudAction = value;
                if (_crudAction == CrudActions.Creating)
                    SelectedSetupParameter = new Setup_Parameter();
            }
        } 
        #endregion Properties

        public ViewModelSetup()
        {
            RefreshCommand = new RelayCommand(RefreshAndDiscardExecMethod, RefreshCanExecMethod);
            SaveCommand = new RelayCommand(SaveExecMethod, SaveAndDiscardCanExecMethod);
            DiscardCommand = new RelayCommand(RefreshAndDiscardExecMethod, SaveAndDiscardCanExecMethod);
            SaveSetupParameterCommand = new RelayCommand(SaveSetupParameterExecMethod, SaveSetupParameterCanExecMethod);
            DiscardSetupParameterCommand = new RelayCommand(DiscardSetupParameterExecMethod, DiscardSetupParameterCanExecMethod);
            Refresh();
        }

        private void GetTables()
        {
            SetupParameters = Dc.Setup_Parameters;
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
        private bool DiscardSetupParameterCanExecMethod(object arg)
        {
            return true;
        }
        private void DiscardSetupParameterExecMethod(object obj)
        {
            if (CrudAction == CrudActions.Creating)
                SelectedSetupParameter = null;
            CloseAction();
        }
        private bool SaveSetupParameterCanExecMethod(object arg)
        {
            if (SelectedSetupParameter == null)
                return false;
            return !string.IsNullOrEmpty(SelectedSetupParameter.Name)
                && !string.IsNullOrEmpty(SelectedSetupParameter.Value)
                && SelectedKindParameter != null;
        }
        private void SaveSetupParameterExecMethod(object obj)
        {
            if (CrudAction == CrudActions.Creating)
                Dc.Setup_Parameters.InsertOnSubmit(SelectedSetupParameter);
            CloseAction();
        }
        #endregion CommandMethods

    }
}
