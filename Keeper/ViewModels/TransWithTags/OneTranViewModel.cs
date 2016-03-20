using System.Composition;
using System.Windows;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.Controls.OneTranViewControls;
using Keeper.Controls.OneTranViewControls.SubControls;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class OneTranViewModel : Screen
    {
        public TranWithTags TranInWork { get; set; }
        public UniversalControlVm MyIncomeControlVm { get; set; }
        public UniversalControlVm MyExpenseControlVm { get; set; }
        public UniversalControlVm MyTransferControlVm { get; set; }
        public OpTypeChoiceControlVm MyOpTypeChoiceControlVm { get; set; } = new OpTypeChoiceControlVm();

        [ImportingConstructor]
        public OneTranViewModel()
        {
            MyIncomeControlVm = IoC.Get<UniversalControlVm>();
            MyExpenseControlVm = IoC.Get<UniversalControlVm>();
            MyTransferControlVm = IoC.Get<UniversalControlVm>();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
        }

        public TranWithTags GetTran()
        {
            return TranInWork;
        }

        private void SetVisibility(OperationType opType)
        {
            MyIncomeControlVm.Visibility = Visibility.Collapsed;
            MyExpenseControlVm.Visibility = Visibility.Collapsed;
            MyTransferControlVm.Visibility = Visibility.Collapsed;
            if (opType == OperationType.Доход) MyIncomeControlVm.Visibility = Visibility.Visible;
            if (opType == OperationType.Расход) MyExpenseControlVm.Visibility = Visibility.Visible;
            if (opType == OperationType.Перенос) MyTransferControlVm.Visibility = Visibility.Visible;
        }
        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran.Clone();
            MyTransferControlVm.SetTran(TranInWork);
            MyIncomeControlVm.SetTran(TranInWork);
            MyExpenseControlVm.SetTran(TranInWork);

            SetVisibility(tran.Operation);

            MyOpTypeChoiceControlVm.PressedButton = TranInWork.Operation;
            MyOpTypeChoiceControlVm.PropertyChanged += MyOpTypeChoiceControlVm_PropertyChanged;
        }

        private void MyOpTypeChoiceControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DisplayName = "Расход";
        }

        public void Save()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }

    }
}
