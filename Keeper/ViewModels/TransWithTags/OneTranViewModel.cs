using System.Composition;
using System.Windows;
using Caliburn.Micro;
using Keeper.Controls;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class OneTranViewModel : Screen
    {
        public TranWithTags TranInWork { get; set; }
        public IncomeControlVm MyIncomeControlVm { get; set; }
        public ExpenseControlVm MyExpenseControlVm { get; set; }
        public OpTypeChoiceControlVm MyOpTypeChoiceControlVm { get; set; } = new OpTypeChoiceControlVm();

        [ImportingConstructor]
        public OneTranViewModel()
        {
            MyIncomeControlVm = IoC.Get<IncomeControlVm>();
            MyExpenseControlVm = IoC.Get<ExpenseControlVm>();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
        }

        public TranWithTags GetTran()
        {
            return TranInWork;
        }
        public void SetTran(TranWithTags tran)
        {
            TranInWork = tran.Clone();
                MyIncomeControlVm.SetTran(TranInWork);
                MyExpenseControlVm.SetTran(TranInWork);
            if (TranInWork.Operation == OperationType.Доход)
            {
                MyIncomeControlVm.Visibility = Visibility.Visible;
                MyExpenseControlVm.Visibility = Visibility.Collapsed;
            }
            else
            {
                MyExpenseControlVm.Visibility = Visibility.Visible;
                MyIncomeControlVm.Visibility = Visibility.Collapsed;
            }
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
