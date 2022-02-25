﻿using System;
using System.Globalization;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public static class BuySellFeeCalculator
    {
        public static Tuple<decimal, CurrencyCode> EvaluatePurchaseFee(this InvestTranModel tran)
        {
            decimal purchaseFee = 0;
            switch (tran.TrustAccount.StockMarket)
            {
                case Market.Russia:
                    {
                        if (tran.InvestOperationType == InvestOperationType.BuyStocks)
                            purchaseFee = Math.Max((decimal)0.0015 * tran.CurrencyAmount, 8);
                        if (tran.InvestOperationType == InvestOperationType.BuyBonds)
                            purchaseFee = Math.Max((decimal)0.0010 * tran.CurrencyAmount, 8);
                        break;
                    }
                case Market.Usa:
                    {
                        if (tran.CurrencyAmount / tran.AssetAmount <= 3)
                            purchaseFee = Math.Max((decimal)0.07 * tran.AssetAmount, 18);
                        else
                            purchaseFee = Math.Max((decimal)0.14 * tran.AssetAmount, 18);
                        break;
                    }
            }

            return new Tuple<decimal, CurrencyCode>(purchaseFee, CurrencyCode.BYN);
        }
    }

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

        public string CurrencyAmountForDatagrid => $"{CurrencyAmount:#,0.00} {Currency.ToString().ToLowerInvariant()}";
        public string FullAmountForDatagrid => $"{CurrencyAmount:#,0.00} + {CouponAmount:#,0.00} = {CurrencyAmount+CouponAmount:#,0.00} {Currency.ToString().ToLowerInvariant()}";
        public string CouponAmountForDatagrid =>
            InvestOperationType == InvestOperationType.BuyBonds
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

        private bool _isBuySellFeePaid;
        public bool IsBuySellFeePaid
        {
            get => _isBuySellFeePaid;
            set
            {
                if (value == _isBuySellFeePaid) return;
                _isBuySellFeePaid = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(BuySellFeeForDataGrid));
            }
        }

        public string BuySellFeeForDataGrid => BuySellFeeToDataGrid();

        private string BuySellFeeToDataGrid()
        {
            if (BuySellFee == 0) return "";

            return $"{BuySellFee} {BuySellFeeCurrency.ToString().ToLower()}";
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
            CouponAmount = source.CouponAmount;
            Currency = source.Currency;
            AssetAmount = source.AssetAmount;
            Asset = source.Asset;
            BuySellFee = source.BuySellFee;
            BuySellFeeCurrency = source.BuySellFeeCurrency;
            IsBuySellFeePaid = source.IsBuySellFeePaid;
            Comment = source.Comment;
        }
    }
}