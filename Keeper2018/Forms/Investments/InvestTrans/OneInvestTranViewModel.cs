using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneInvestTranViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly ComboTreesProvider _comboTreesProvider;
        private readonly BalanceDuringTransactionHinter _balanceDuringTransactionHinter;

        public Visibility AccountVisibility { get; set; }
        public Visibility AssetVisibility { get; set; }
        public Visibility AssetAmountVisibility { get; set; }

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

        public string TrustAccountLabel { get; set; }
        public List<TrustAccount> TrustAccounts { get; set; }
        public string AssetLabel { get; set; }
        public List<InvestmentAsset> Assets { get; set; }

        public InvestTranModel TranInWork { get; set; } = new InvestTranModel();

        public OneInvestTranViewModel(KeeperDataModel dataModel, ComboTreesProvider comboTreesProvider,
            BalanceDuringTransactionHinter balanceDuringTransactionHinter)
        {
            _dataModel = dataModel;
            _comboTreesProvider = comboTreesProvider;
            _comboTreesProvider.Initialize();
            _balanceDuringTransactionHinter = balanceDuringTransactionHinter;
            TrustAccounts = dataModel.TrustAccounts;
            Assets = dataModel.InvestmentAssets;
        }


        public void Initialize(InvestTranModel tran)
        {
            TranInWork = tran;

            switch (tran.InvestOperationType)
            {
                case InvestOperationType.TopUpTrustAccount:
                    AccountVisibility = Visibility.Visible;
                    AssetVisibility = Visibility.Collapsed;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm.InitializeForInvestments(tran, _comboTreesProvider);
                    TrustAccountLabel = "На трастовый счет";
                    break;
                case InvestOperationType.BuyStocks:
                    AccountVisibility = Visibility.Collapsed;
                    AssetVisibility = Visibility.Visible;
                    AssetAmountVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = null;
                    TrustAccountLabel = "С трастового счета";
                    break;
                case InvestOperationType.PayBaseCommission:
                    AccountVisibility = Visibility.Visible;
                    AssetVisibility = Visibility.Collapsed;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm.InitializeForInvestments(tran, _comboTreesProvider);
                    TrustAccountLabel = "По трастовому счету";
                    TranInWork.Currency = CurrencyCode.BYN;
                    break;
                case InvestOperationType.EnrollCouponOrDividends:
                    AccountVisibility = Visibility.Visible;
                    AssetVisibility = Visibility.Visible;
                    AssetAmountVisibility = Visibility.Collapsed;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm.InitializeForInvestments(tran, _comboTreesProvider);
                    TrustAccountLabel = "На трастовый счет";
                    AssetLabel = "По бумаге";
                    break;

            }
            MyDatePickerVm.SelectedDate = TranInWork.Timestamp;

            TranInWork.PropertyChanged += TranInWork_PropertyChanged;
        }

        private void TranInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TrustAccount")
            {
                if (TranInWork.InvestOperationType == InvestOperationType.TopUpTrustAccount)
                {
                    TranInWork.Currency = TranInWork.TrustAccount.Currency;
                }
            }
        }

        public void Save()
        {
            TranInWork.Timestamp = MyDatePickerVm.SelectedDate;
            TranInWork.AccountModel = MyAccNameSelectorVm == null ? null : _dataModel.AcMoDict[MyAccNameSelectorVm.MyAccName.Id];
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
