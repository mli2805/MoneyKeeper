using System.Collections.Generic;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneInvestTranViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly ComboTreesProvider _comboTreesProvider;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

        private AccNameSelectorVm _myAccNameSelectorVm = new AccNameSelectorVm();
        public AccNameSelectorVm MyAccNameSelectorVm
        {
            get => _myAccNameSelectorVm;
            set
            {
                if (Equals(value, _myAccNameSelectorVm)) return;
                _myAccNameSelectorVm = value;
                NotifyOfPropertyChange();
            }
        }

        private DatePickerWithTrianglesVm _myDatePickerVm = new DatePickerWithTrianglesVm();
        public DatePickerWithTrianglesVm MyDatePickerVm
        {
            get => _myDatePickerVm;
            set
            {
                if (Equals(value, _myDatePickerVm)) return;
                _myDatePickerVm = value;
                NotifyOfPropertyChange();
            }
        }

        public List<TrustAccount> TrustAccounts { get; set; }

        public InvestTranModel TranInWork { get; set; } = new InvestTranModel();

        public OneInvestTranViewModel(KeeperDataModel dataModel, ComboTreesProvider comboTreesProvider,
            BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _dataModel = dataModel;
            _comboTreesProvider = comboTreesProvider;
            _comboTreesProvider.Initialize();
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            TrustAccounts = dataModel.TrustAccounts;
        }


        public void Initialize(InvestTranModel tran)
        {
            TranInWork = tran;
            MyAccNameSelectorVm.InitializeForInvestments(tran, _comboTreesProvider);
            MyDatePickerVm.SelectedDate = TranInWork.Timestamp;
        }

        public void Save()
        {
            TranInWork.Timestamp = MyDatePickerVm.SelectedDate;
            TranInWork.AccountModel = _dataModel.AcMoDict[MyAccNameSelectorVm.MyAccName.Id];
            TranInWork.Currency = TranInWork.TrustAccount.Currency;
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
