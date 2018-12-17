using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrafficOfBranchCalculator : ITraffic
    {
        private readonly KeeperDb _db;
        private readonly AccountModel _accountModel;
        private readonly Period _period;
        private readonly BalanceWithTurnoverOfBranch _balanceWithTurnovers = new BalanceWithTurnoverOfBranch();
        public decimal TotalAmount;
        public string Total => TotalAmount.ToString("0.## usd");

        public TrafficOfBranchCalculator(KeeperDb db, AccountModel accountModel, Period period)
        {
            _db = db;
            _accountModel = accountModel;
            _period = period;
        }

        public void Evaluate()
        {
            foreach (var tran in _db.TransactionModels.Where(t => _period.Includes(t.Timestamp)))
                RegisterTran(tran);
            TotalAmount = _db.BalanceInUsd(_period.FinishMoment, _balanceWithTurnovers.Balance());
        }

        private void RegisterTran(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _balanceWithTurnovers.Add(myAcc, tran.Currency, tran.Amount);
                    }
                    break;
                case OperationType.Расход:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _balanceWithTurnovers.Sub(myAcc, tran.Currency, tran.Amount);
                    }
                    break;
                case OperationType.Перенос:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _balanceWithTurnovers.Sub(myAcc, tran.Currency, tran.Amount);
                        var myAcc2 = tran.MySecondAccount.IsC(_accountModel);
                        if (myAcc2 != null)
                            _balanceWithTurnovers.Add(myAcc2, tran.Currency, tran.Amount);
                    }
                    break;
                case OperationType.Обмен:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _balanceWithTurnovers.Sub(myAcc, tran.Currency, tran.Amount);
                        var myAcc2 = tran.MySecondAccount.IsC(_accountModel);
                        if (myAcc2 != null)
                            // ReSharper disable once PossibleInvalidOperationException
                            _balanceWithTurnovers.Add(myAcc2, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    }
                    break;
            }
        }

        public IEnumerable<string> Report(BalanceOrTraffic mode) { return _balanceWithTurnovers.Report(mode); }

        public List<string> ReportForMonthAnalysis()
        {
            var result = new List<string>();

            var root = _balanceWithTurnovers.Summarize().Balance();
            result.AddRange(_db.BalanceReport(_period.FinishMoment, root));

            foreach (var pair in _balanceWithTurnovers.ChildAccounts)
            {
                result.Add("");
                result.Add($"   {pair.Key.Name}");
                var child = pair.Value.Balance();
                result.AddRange(_db.BalanceReport(_period.FinishMoment, child).Select(line => $"   {line}"));
            }

            return result;
        }
    }
}