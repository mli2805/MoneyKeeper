using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TrustAccountAnalysisViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private TrustAccount _trustAccount;

        public List<InvestmentAssetOnDate> Rows { get; set; } = new List<InvestmentAssetOnDate>();
        public decimal AllCurrentActives { get; set; }
        public decimal Cash { get; set; }
        public decimal AllPaidFees { get; set; }
        public decimal BaseFee { get; set; }
        public decimal OperationsFee { get; set; }
        public decimal NotPaidFees { get; set; }

        public string FinResult { get; set; }
        public string FinPercent { get; set; }

        public InvestmentAssetOnDate Total { get; set; }
        public string Expense { get; set; }
        public string Fees { get; set; }
        public string Externals { get; set; }
        public string AllCurrentActivesStr { get; set; }

        public TrustAccountAnalysisViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = $"{_trustAccount.Title}";
        }

        public void Initialize(TrustAccount trustAccount)
        {
            _trustAccount = trustAccount;
            var bal = _dataModel.GetTrustAccountBalance(trustAccount, DateTime.Today);
            foreach (var asset in bal.Assets)
            {
                asset.CurrentPriceOfOne = _dataModel.AssetRates.Last(r => r.TickerId == asset.InvestmentAssetId).Value;
                asset.CurrentPrice = asset.CurrentPriceOfOne * asset.Quantity;
                asset.FinResult = asset.CurrentPrice - asset.Price - asset.PaidCoupon + asset.ReceivedCoupon;
            }

            Rows.Clear();
            Rows.AddRange(bal.Assets);
            Cash = bal.Cash;
            NotPaidFees = bal.NotPaidFees;
            BaseFee = bal.BaseFee;
            OperationsFee = bal.Assets.Sum(a => a.BuySellFeeInTrustCurrency);
            AllCurrentActives = bal.Assets.Sum(a => a.CurrentPrice);

            EvaluateTotals(bal);
        }

        private void EvaluateTotals(TrustAccountBalanceOnDate bal)
        {
            Total = new InvestmentAssetOnDate() { InvestmentCurrency = _trustAccount.Currency };

            Total.Price = Rows.Sum(r => r.Price);

            Total.BuySellFee = Rows.Sum(r => r.BuySellFee);
            Total.BuySellFeeCurrency = CurrencyCode.BYN;
            Total.BuySellFeeInTrustCurrency = Rows.Sum(r => r.BuySellFeeInTrustCurrency);

            Total.PaidCoupon = Rows.Sum(r => r.PaidCoupon);
            Total.ReceivedCoupon = Rows.Sum(r => r.ReceivedCoupon);

            Total.CurrentPrice = Rows.Sum(r => r.CurrentPrice);
            Total.FinResult = Rows.Sum(r => r.FinResult);

            AllPaidFees = OperationsFee + bal.BaseFee;
            Expense = $"{Total.Price:N} + {Total.PaidCoupon:N} = {Total.Price + Total.PaidCoupon:N}";
            Fees = $"{Total.BuySellFeeInTrustCurrency:N} + {bal.BaseFee:N} = {AllPaidFees:N}";
            var fullExpense = Total.Price + Total.PaidCoupon + AllPaidFees;

            var externals = bal.TopUp + AllPaidFees - bal.Withdraw;
            Externals = $"{bal.TopUp:N} + {AllPaidFees:N} - {bal.Withdraw:N} = {externals:N}";
            AllCurrentActivesStr = $"{Cash:N} + {AllCurrentActives:N} = {AllCurrentActives + Cash:N}";

            var result = AllCurrentActives + Cash - externals;
            FinResult = $"{AllCurrentActives + Cash:N} - {externals:N} = {result:N}";
            FinPercent = $" {result / externals * 100:N}%  /  {result / fullExpense * 100:N}%";
        }
    }
}
