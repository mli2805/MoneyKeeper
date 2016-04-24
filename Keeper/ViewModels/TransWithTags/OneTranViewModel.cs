using System.Composition;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.Controls.OneTranViewControls;
using Keeper.Controls.OneTranViewControls.SubControls;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class OneTranViewModel : Screen
    {
        private readonly AccountTreeStraightener _accountTreeStraightener;
        public TranWithTags TranInWork { get; set; }
        public UniversalControlVm MyIncomeControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyExpenseControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyTransferControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyExchangeControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public OpTypeChoiceControlVm MyOpTypeChoiceControlVm { get; set; } = new OpTypeChoiceControlVm();

        [ImportingConstructor]
        public OneTranViewModel(AccountTreeStraightener accountTreeStraightener)
        {
            _accountTreeStraightener = accountTreeStraightener;
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

            SetAndShowCorrespondingControl();

            MyOpTypeChoiceControlVm.PressedButton = TranInWork.Operation;
            MyOpTypeChoiceControlVm.PropertyChanged += MyOpTypeChoiceControlVm_PropertyChanged;
        }

        private void MyOpTypeChoiceControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TranInWork.Operation = ((OpTypeChoiceControlVm)sender).PressedButton;
            ValidateTranInWorkFieldsWithNewOperationType();
            SetAndShowCorrespondingControl();
        }

        private void ValidateTranInWorkFieldsWithNewOperationType()
        {

            if (TranInWork.MySecondAccount == null)
            {
                if (TranInWork.Operation == OperationType.Перенос)
                {
//                   _accountTreeStraightener.Seek("Юлин кошелек", ListsForComboTrees.MyAccNamesForTransfer) 
                }
            }
        }

        public void Save()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }

        private void SetAndShowCorrespondingControl()
        {
            if (TranInWork.Operation == OperationType.Доход)   MyIncomeControlVm.SetTran(TranInWork); 
            if (TranInWork.Operation == OperationType.Расход)  MyExpenseControlVm.SetTran(TranInWork); 
            if (TranInWork.Operation == OperationType.Перенос) MyTransferControlVm.SetTran(TranInWork);
            if (TranInWork.Operation == OperationType.Обмен)   MyExchangeControlVm.SetTran(TranInWork);
            SetVisibility(TranInWork.Operation);
        }
        private void SetVisibility(OperationType opType)
        {
            MyIncomeControlVm.Visibility = opType == OperationType.Доход ? Visibility.Visible : Visibility.Collapsed;
            MyExpenseControlVm.Visibility = opType == OperationType.Расход ? Visibility.Visible : Visibility.Collapsed;
            MyTransferControlVm.Visibility = opType == OperationType.Перенос ? Visibility.Visible : Visibility.Collapsed;
            MyExchangeControlVm.Visibility = opType == OperationType.Обмен ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
