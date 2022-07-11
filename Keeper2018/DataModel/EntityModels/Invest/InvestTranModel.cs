using System;
using System.Windows.Media;
using System.Globalization;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class InvestTranModel : PropertyChangedBase
    {
        public int Id { get; set; }

        private InvestOperationType _investOperationType;
        public InvestOperationType InvestOperationType
        {
            get => _investOperationType;
            set
            {
                if (value == _investOperationType) return;
                _investOperationType = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TypeForDatagrid));
            }
        }

        public string TypeForDatagrid => InvestOperationType.GetRussian();

        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                if (value.Equals(_timestamp)) return;
                _timestamp = value;
                NotifyOfPropertyChange();
            }
        }

        public string TranForCombo => $"{Timestamp:d} {CurrencyAmount} {Currency.ToString().ToLower()}";

        private AccountModel _accountModel;
        public AccountModel AccountModel
        {
            get => _accountModel;
            set
            {
                if (Equals(value, _accountModel)) return;
                _accountModel = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(AccountForDataGrid));
            }
        }

        public string AccountForDataGrid => AccountModel?.Name ?? "";

        private TrustAccount _trustAccount;
        public TrustAccount TrustAccount
        {
            get => _trustAccount;
            set
            {
                if (Equals(value, _trustAccount)) return;
                _trustAccount = value;
                NotifyOfPropertyChange();
            }
        }

        private decimal _currencyAmount;
        public decimal CurrencyAmount
        {
            get => _currencyAmount;
            set
            {
                if (value == _currencyAmount) return;
                _currencyAmount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CurrencyAmountForDatagrid));
            }
        }

        private decimal _couponAmount;
        public decimal CouponAmount
        {
            get => _couponAmount;
            set
            {
                if (value == _couponAmount) return;
                _couponAmount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CouponAmountForDatagrid));
            }
        }

        public decimal FullAmount => CurrencyAmount + CouponAmount;

        private CurrencyCode _currency;
        public CurrencyCode Currency
        {
            get => _currency;
            set
            {
                if (value == _currency) return;
                _currency = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CurrencyAmountForDatagrid));
            }
        }

        public decimal Rate; // to convert sum into $, filled in and used in analysis

        public string CurrencyAmountForDatagrid => $"{CurrencyAmount:#,0.00} {Currency.ToString().ToLowerInvariant()}";
        public string FullAmountForDatagrid => $"{CurrencyAmount:#,0.00} + {CouponAmount:#,0.00} = {CurrencyAmount+CouponAmount:#,0.00} {Currency.ToString().ToLowerInvariant()}";
        public string CouponAmountForDatagrid =>
            InvestOperationType == InvestOperationType.BuyBonds
            || InvestOperationType == InvestOperationType.SellBonds
                ? $"{CouponAmount:#,0.00} {Currency.ToString().ToLowerInvariant()}"
                : "";

        private int _assetAmount;
        public int AssetAmount
        {
            get => _assetAmount;
            set
            {
                if (value.Equals(_assetAmount)) return;
                _assetAmount = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(AssetAmountForDatagrid));
            }
        }

        public string AssetAmountForDatagrid =>
            AssetAmount == 0 ? "" : AssetAmount.ToString(CultureInfo.CurrentCulture);

        private InvestmentAssetModel _asset;
        public InvestmentAssetModel Asset
        {
            get => _asset;
            set
            {
                if (Equals(value, _asset)) return;
                _asset = value;
                NotifyOfPropertyChange();
            }
        }

        public string AssetForDatagrid => AssetToTable();

        private string AssetToTable()
        {
            switch (InvestOperationType)
            {
                case InvestOperationType.BuyBonds:
                case InvestOperationType.BuyStocks:
                case InvestOperationType.SellBonds:
                case InvestOperationType.SellStocks:
                case InvestOperationType.EnrollCouponOrDividends:
                    return $"{Asset.Ticker}::{Asset.Title}";
                default: return "";
            }
        }

        private decimal _buySellFee;
        public decimal BuySellFee
        {
            get => _buySellFee;
            set
            {
                if (value == _buySellFee) return;
                _buySellFee = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(BuySellFeeForDataGrid));
            }
        }

        private CurrencyCode _buySellFeeCurrency = CurrencyCode.BYN;
        public CurrencyCode BuySellFeeCurrency
        {
            get => _buySellFeeCurrency;
            set
            {
                if (value == _buySellFeeCurrency) return;
                _buySellFeeCurrency = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(BuySellFeeForDataGrid));
            }
        }

        private int _feePaymentOperationId;
        public int FeePaymentOperationId
        {
            get => _feePaymentOperationId;
            set
            {
                if (value == _feePaymentOperationId) return;
                _feePaymentOperationId = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(BuySellFeeForDataGrid));
            }
        }

        public string BuySellFeeForDataGrid => BuySellFeeToDataGrid();

        private string BuySellFeeToDataGrid()
        {
            if (BuySellFee == 0) return "";
            var paid = FeePaymentOperationId != 0 ? "оплач. " : "";

            return $"{paid}{BuySellFee} {BuySellFeeCurrency.ToString().ToLower()}";
        }

        private string _comment = "";

        public string Comment
        {
            get => _comment;
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        public Brush TransactionFontColor => InvestOperationType.FontColor();
    
        #region ' _isSelected '
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }
        #endregion
        public InvestTranModel ShallowCopy()
        {
            return (InvestTranModel)MemberwiseClone();
        }

        public void CopyFieldsFrom(InvestTranModel source)
        {
            Id = source.Id;
            InvestOperationType = source.InvestOperationType;
            Timestamp = source.Timestamp;
            AccountModel = source.AccountModel;
            TrustAccount = source.TrustAccount;
            CurrencyAmount = source.CurrencyAmount;
            CouponAmount = source.CouponAmount;
            Currency = source.Currency;
            AssetAmount = source.AssetAmount;
            Asset = source.Asset;
            BuySellFee = source.BuySellFee;
            BuySellFeeCurrency = source.BuySellFeeCurrency;
            FeePaymentOperationId = source.FeePaymentOperationId;
            Comment = source.Comment;
        }
    }
}