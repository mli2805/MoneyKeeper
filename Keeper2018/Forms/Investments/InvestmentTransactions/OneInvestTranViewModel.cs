using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class OneInvestTranViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly AccNameSelector _accNameSelectorForInvestment;

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
        public List<InvestmentAssetModel> Assets { get; set; }

        public InvestTranModel TranInWork { get; set; } = new InvestTranModel();

        public List<InvestTranModel> FeePayments { get; set; }

        public OneInvestTranViewModel(KeeperDataModel dataModel, ComboTreesProvider comboTreesProvider,
            AccNameSelector accNameSelectorForInvestment)
        {
            _dataModel = dataModel;
            _accNameSelectorForInvestment = accNameSelectorForInvestment;
            comboTreesProvider.Initialize();
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
            FeePayments = _dataModel.InvestTranModels
                .Where(t => t.InvestOperationType == InvestOperationType.PayBuySellFee
                        && t.TrustAccount == TranInWork.TrustAccount).ToList();

            switch (tran.InvestOperationType)
            {
                case InvestOperationType.TopUpTrustAccount:
                    AccountVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm = _accNameSelectorForInvestment.InitializeForInvestments(tran);
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
                    TrustAccountLabel = "С трастового счета"; 
                    break;
                case InvestOperationType.PayBaseCommission:
                    AccountVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm = _accNameSelectorForInvestment.InitializeForInvestments(tran);
                    CurrencyAmountText = "Сумма";
                    TrustAccountLabel = "По трастовому счету";
                    TranInWork.Currency = CurrencyCode.BYN;
                    break;
                case InvestOperationType.PayBuySellFee:
                    AccountVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm = _accNameSelectorForInvestment.InitializeForInvestments(tran);
                    CurrencyAmountText = "Сумма";
                    TrustAccountLabel = "По трастовому счету";
                    TranInWork.Currency = CurrencyCode.BYN;
                    break;
                case InvestOperationType.SellBonds:
                    AssetVisibility = Visibility.Visible;
                    CouponVisibility = Visibility.Visible;
                    AssetAmountVisibility = Visibility.Visible;
                    BuySellFeeVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = null;
                    TrustAccountLabel = "На трастовый счет"; 
                    break;
                case InvestOperationType.SellStocks:
                    AssetVisibility = Visibility.Visible;
                    AssetAmountVisibility = Visibility.Visible;
                    BuySellFeeVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = null;
                    TrustAccountLabel = "На трастовый счет";
                    break;
                case InvestOperationType.EnrollCouponOrDividends:
                    AccountVisibility = Visibility.Visible;
                    AssetVisibility = Visibility.Visible;
                    AssetAmountVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm = _accNameSelectorForInvestment.InitializeForInvestments(tran);
                    CurrencyAmountText = "Сумма";
                    TrustAccountLabel = "На трастовый счет";
                    AssetLabel = "По бумаге";
                    break;
                case InvestOperationType.WithdrawFromTrustAccount:
                    AccountVisibility = Visibility.Visible;
                    MyAccNameSelectorVm = new AccNameSelectorVm();
                    MyAccNameSelectorVm = _accNameSelectorForInvestment.InitializeForInvestments(tran);
                    CurrencyAmountText = "Сумма";
                    TrustAccountLabel = "С трастового счета";
                    break;
            }
            MyDatePickerVm.SelectedDate = TranInWork.Timestamp;

            TranInWork.PropertyChanged += TranInWork_PropertyChanged;
        }

        private void TranInWork_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TrustAccount")
            {
                TranInWork.Currency = TranInWork.InvestOperationType == InvestOperationType.PayBaseCommission
                                        || TranInWork.InvestOperationType == InvestOperationType.PayBuySellFee 
                    ? CurrencyCode.BYN 
                    : TranInWork.TrustAccount.Currency;
                FeePayments = _dataModel.InvestTranModels
                    .Where(t => t.InvestOperationType == InvestOperationType.PayBuySellFee
                                && t.TrustAccount == TranInWork.TrustAccount).ToList();
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
