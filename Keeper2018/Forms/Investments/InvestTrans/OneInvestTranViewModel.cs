using Caliburn.Micro;

namespace Keeper2018
{
    public class OneInvestTranViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;
        private readonly AccNameSelectionControlInitializer _accNameSelectionControlInitializer;

        private AccNameSelectorVm _myAccNameSelectorVm;
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

        private AmountInputControlVm _myAmountInputControlVm;
        public AmountInputControlVm MyAmountInputControlVm
        {
            get => _myAmountInputControlVm;
            set
            {
                if (Equals(value, _myAmountInputControlVm)) return;
                _myAmountInputControlVm = value;
                NotifyOfPropertyChange();
            }
        }

        private DatePickerWithTrianglesVm _myDatePickerVm;
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

        public InvestTranModel TranInWork { get; set; } = new InvestTranModel();

        public OneInvestTranViewModel(KeeperDataModel dataModel, BalanceDuringTransactionHinter balanceDuringTransactionHinter,
            AccNameSelectionControlInitializer accNameSelectionControlInitializer)
        {
            _dataModel = dataModel;
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            _accNameSelectionControlInitializer = accNameSelectionControlInitializer;
        }


        public void Initialize(InvestTranModel tran)
        {
            TranInWork = tran;
            // MyAccNameSelectorVm = _accNameSelectionControlInitializer.ForMyAccount(TranInWork);

        }
    }
}
