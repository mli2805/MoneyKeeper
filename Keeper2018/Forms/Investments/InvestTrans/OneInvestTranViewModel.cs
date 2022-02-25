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

        public Visibility AccountVisibility { get; set; } = Visibility.Collapsed;
        public Visibility AssetVisibility { get; set; } = Visibility.Collapsed;
        public Visibility CouponVisibility { get; set; } = Visibility.Collapsed;
        public Visibility AssetAmountVisibility { get; set; } = Visibility.Collapsed;
        public Visibility BuySellFeeVisibility { get; set; } = Visibility.Collapsed;

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

        public string CurrencyAmountText { get; set; } = "";

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

        public OneInvestTranViewModel(KeeperDataModel dataModel, ComboTreesProvider comboTreesProvider)
        {
            _dataModel = dataModel;
            _comboTreesProvider = comboTreesProvider;
            _comboTreesProvider.Initialize();
            TrustAccounts = dataModel.TrustAccounts;
            Assets = dataModel.InvestmentAssets;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = TranInWork.InvestOperationType.GetRussian();
        }

        public void Initialize(InvestTranModel tran)
        {
            TranInWork = tran;

            switch (tran.InvestOperationType)
            {
                case InvestOperationType.TopUpTrustAccount:
                    AccountVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm.InitializeForInvestments(tran, _comboTreesProvider);
                    CurrencyAmountText = "Сумма";
                    TrustAccountLabel = "На трастовый счет";
                    break;
                case InvestOperationType.BuyStocks:
                    CurrencyAmountText = "Сумма";
                    AssetVisibility = Visibility.Visible;
                    AssetAmountVisibility = Visibility.Visible;
                    BuySellFeeVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = null;
                    TrustAccountLabel = "С трастового счета";
                    break;
                case InvestOperationType.BuyBonds:
                    CurrencyAmountText = "За  сами  облигации";
                    CouponVisibility = Visibility.Visible;
                    AssetVisibility = Visibility.Visible;
                    AssetAmountVisibility = Visibility.Visible;
                    BuySellFeeVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = null;
                    TrustAccountLabel = "С трастового счета"; break;
                case InvestOperationType.PayBaseCommission:
                    AccountVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm.InitializeForInvestments(tran, _comboTreesProvider);
                    CurrencyAmountText = "Сумма";
                    TrustAccountLabel = "По трастовому счету";
                    TranInWork.Currency = CurrencyCode.BYN;
                    break;
                case InvestOperationType.EnrollCouponOrDividends:
                    AccountVisibility = Visibility.Visible;
                    AssetVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm.InitializeForInvestments(tran, _comboTreesProvider);
                    CurrencyAmountText = "Сумма";
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
                TranInWork.Currency = TranInWork.TrustAccount.Currency;
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
