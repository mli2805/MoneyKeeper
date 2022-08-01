using System;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TrustAccountAnalysisViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private TrustAccount _trustAccount;

        public AssetsTableViewModel OpenAssetsViewModel { get; set; } = new AssetsTableViewModel();
        public AssetsTableViewModel ClosedAssetsViewModel { get; set; } = new AssetsTableViewModel();

        public decimal AllCurrentActives { get; set; }
        public decimal Cash { get; set; }
        public decimal AllPaidFees { get; set; }
        public decimal BaseFee { get; set; }
        public decimal OperationsFee { get; set; }
        public decimal NotPaidFees { get; set; }

        public string FinResult { get; set; }
        public string FinPercent { get; set; }

        public string Expense { get; set; }
        public string Fees { get; set; }
        public string TransferredToTrust { get; set; }
        public string RealExternals { get; set; }
        public string FreeCash { get; set; }
        public string AllCurrentActivesStr { get; set; }

        public TrustAccountAnalysisViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = $"{_trustAccount.Title}";
        }

        public void Initialize(TrustAccount trustAccount, DateTime date)
        {
            _trustAccount = trustAccount;
            var bal = _dataModel.GetTrustAccountBalance(trustAccount, date);
            OpenAssetsViewModel.Initialize(bal, _dataModel, _trustAccount, date, true);
            ClosedAssetsViewModel.Initialize(bal, _dataModel, _trustAccount, date, false);

            Cash = bal.Cash;
            NotPaidFees = bal.NotPaidFees;
            BaseFee = bal.BaseFee;
            OperationsFee = bal.Assets.Sum(a => a.BuySellFeeInTrustCurrency);
            AllCurrentActives = bal.Assets.Sum(a => a.CurrentPrice) + bal.Assets.Sum(a=>a.AccumulatedCoupon);

            EvaluateTotals(bal, OpenAssetsViewModel.Total);
        }

        private void EvaluateTotals(TrustAccountBalanceOnDate bal, InvestmentAssetOnDate total)
        {
            AllPaidFees = OperationsFee + bal.BaseFee;
            Expense = $"{total.Price - total.PaidCoupon:N} + {total.PaidCoupon:N} = {total.Price:N}";
            Fees = $"{total.BuySellFeeInTrustCurrency:N} + {bal.BaseFee:N} = {AllPaidFees:N}";
            var fullExpense = total.Price + total.PaidCoupon + AllPaidFees;

            var transferred = bal.TopUp - bal.Withdraw;
            TransferredToTrust = $"{bal.TopUp:N} - {bal.Withdraw:N} = {transferred:N}";

            var externals = bal.TopUp + AllPaidFees - bal.ReceivedCoupon - bal.Withdraw;
            RealExternals = $"{bal.TopUp:N} + {AllPaidFees:N} - {bal.ReceivedCoupon:N} - {bal.Withdraw:N} = {externals:N}";

            FreeCash = $"{Cash:N}";
            AllCurrentActivesStr = $"{AllCurrentActives:N} + {Cash:N} = {AllCurrentActives + Cash:N}";

            var result = AllCurrentActives + Cash - externals;
            FinResult = $"{AllCurrentActives + Cash:N} - {externals:N} = {result:N}";
            FinPercent = $" {result / externals * 100:N}%  /  {result / fullExpense * 100:N}%";
        }

      
    }
}
