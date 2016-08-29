using System.Composition;
using System.Windows;
using Caliburn.Micro;
using Keeper.Controls.OneTranViewControls;
using Keeper.Controls.OneTranViewControls.SubControls;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.Utils.AccountEditing;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class OneTranViewModel : Screen
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private string _caption;
        private TranWithTags _tranInWork;

        public TranWithTags TranInWork
        {
            get { return _tranInWork; }
            set
            {
                if (Equals(value, _tranInWork)) return;
                _tranInWork = value;
                NotifyOfPropertyChange();
            }
        }

        public UniversalControlVm MyIncomeControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyExpenseControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyTransferControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public UniversalControlVm MyExchangeControlVm { get; set; } = IoC.Get<UniversalControlVm>();
        public OpTypeChoiceControlVm MyOpTypeChoiceControlVm { get; set; } = new OpTypeChoiceControlVm();

        [ImportingConstructor]
        public OneTranViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public TranWithTags GetTran()
        {
            return TranInWork;
        }

        public void Init(TranWithTags tran, string caption)
        {
            _caption = caption;
            SetTran(tran);
        }
        private void SetTran(TranWithTags tran)
        {
            TranInWork = tran.Clone();

            SetVisibility(TranInWork.Operation);
            InitCorrespondingControl();

            MyOpTypeChoiceControlVm.PressedButton = TranInWork.Operation;
            MyOpTypeChoiceControlVm.PropertyChanged += MyOpTypeChoiceControlVm_PropertyChanged;
        }

        private void MyOpTypeChoiceControlVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TranInWork.Operation = ((OpTypeChoiceControlVm)sender).PressedButton;
            ValidateTranInWorkFieldsWithNewOperationType();
            SetVisibility(TranInWork.Operation);
            InitCorrespondingControl();
        }

        private void ValidateTranInWorkFieldsWithNewOperationType()
        {
            TranInWork.Tags.Clear();
            if (TranInWork.MySecondAccount == null)
            {
                if (TranInWork.Operation == OperationType.Перенос)
                {
                   TranInWork.MySecondAccount = _accountTreeStraightener.Seek("Юлин кошелек", _db.Accounts);
                }
            }
            if (TranInWork.CurrencyInReturn == null)
            {
                TranInWork.CurrencyInReturn = (TranInWork.Currency == CurrencyCodes.BYR) ? CurrencyCodes.USD : CurrencyCodes.BYR; 
            }
        }

        private bool IsValid()
        {
            if (TranInWork.HasntGotCategoryTagThoughItShould()) return false;
            /* more checks
             * ...
            */
            return true;
        }

        public void Save()
        {
            if (!IsValid()) return;
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }

        private void InitCorrespondingControl()
        {
//            if (TranInWork.Operation == OperationType.Доход)   MyIncomeControlVm.SetTran(TranInWork); 
//            if (TranInWork.Operation == OperationType.Расход)  MyExpenseControlVm.SetTran(TranInWork); 
//            if (TranInWork.Operation == OperationType.Перенос) MyTransferControlVm.SetTran(TranInWork);
//           if (TranInWork.Operation == OperationType.Обмен)   MyExchangeControlVm.SetTran(TranInWork);
             MyIncomeControlVm.SetTran(TranInWork);
             MyExpenseControlVm.SetTran(TranInWork);
             MyTransferControlVm.SetTran(TranInWork);
             MyExchangeControlVm.SetTran(TranInWork);
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
