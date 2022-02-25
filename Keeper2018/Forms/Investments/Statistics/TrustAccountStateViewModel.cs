using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TrustAccountStateViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private TrustAccount _trustAccount;

        public List<TrustAccountLine> Rows { get; private set; }

        public string TopUp { get; set; }
        public string Buy { get; set; }
        public string Enroll { get; set; }
        public string Withdraw { get; set; }
        public string Balance { get; set; }


        public TrustAccountStateViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _trustAccount.ToCombo;
        }

        public void Evaluate(TrustAccount trustAccount)
        {
            _trustAccount = trustAccount;

            Rows = new List<TrustAccountLine>();
            decimal balanceBefore = 0;

            foreach (var tr in _dataModel.InvestTranModels.Where(t => t.TrustAccount.Id == trustAccount.Id))
            {
                var line = tr.Create(balanceBefore);
                Rows.Add(line);
                balanceBefore = line.BalanceAfter;
            }

            var topUp = _dataModel.InvestTranModels
                .Where(t => t.TrustAccount.Id == trustAccount.Id &&
                            t.InvestOperationType == InvestOperationType.TopUpTrustAccount)
                .Sum(r => r.CurrencyAmount);
            var buy = _dataModel.InvestTranModels
                .Where(t => t.TrustAccount.Id == trustAccount.Id &&
                            (t.InvestOperationType == InvestOperationType.BuyBonds || t.InvestOperationType == InvestOperationType.BuyStocks))
                .Sum(r => r.FullAmount);
            var enroll = _dataModel.InvestTranModels
                .Where(t => t.TrustAccount.Id == trustAccount.Id &&
                            t.InvestOperationType == InvestOperationType.EnrollCouponOrDividends)
                .Sum(r => r.CurrencyAmount);
            var withdraw = _dataModel.InvestTranModels
                .Where(t => t.TrustAccount.Id == trustAccount.Id &&
                            t.InvestOperationType == InvestOperationType.WithdrawFromTrustAccount)
                .Sum(r => r.CurrencyAmount);

            TopUp = $"{topUp:#,0.00}";
            Buy = $"{buy:#,0.00}";
            Enroll = $"{enroll:#,0.00}";
            Withdraw = $"{withdraw:#,0.00}";
            Balance = $"{topUp - buy + enroll - withdraw:#,0.00}";
        }
    }
}
