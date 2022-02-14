using System;
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

        public string CurrencyAmountForDatagrid => $"{CurrencyAmount} {Currency.ToString().ToLowerInvariant()}";

        private double _assetAmount;
        public double AssetAmount
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

        private InvestmentAsset _asset;
        public InvestmentAsset Asset
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
                case InvestOperationType.EnrollCouponOrDividends:
                    return $"{Asset.Ticker}::{Asset.Title}";
                default: return "";
            }
        }

        private string _comment;
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
            Currency = source.Currency;
            AssetAmount = source.AssetAmount;
            Asset = source.Asset;
            Comment = source.Comment;
        }
    }
}