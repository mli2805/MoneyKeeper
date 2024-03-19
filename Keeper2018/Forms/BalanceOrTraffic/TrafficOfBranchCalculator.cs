using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using KeeperDomain;

namespace Keeper2018
{
    public class TrafficOfBranchCalculator : ITraffic
    {
        private readonly KeeperDataModel _dataModel;
        private readonly AccountItemModel _accountItemModel;
        private readonly Period _period;
        private readonly BalanceWithTurnoverOfBranch _balanceWithTurnovers = new BalanceWithTurnoverOfBranch();
        public decimal TotalAmount;
       

        public string Total => TotalAmount.ToString("0.## usd");

        public TrafficOfBranchCalculator(KeeperDataModel dataModel, AccountItemModel accountItemModel, Period period)
        {
            _dataModel = dataModel;
            _accountItemModel = accountItemModel;
            _period = period;
        }

        public void EvaluateAccount()
        {
            foreach (var tran in _dataModel.Transactions.Values.Where(t => _period.Includes(t.Timestamp)))
                RegisterTran(tran);
            TotalAmount = _dataModel.BalanceInUsd(_period.FinishMoment, _balanceWithTurnovers.Balance());
        }

        public Balance Evaluate()
        {
            foreach (var tran in _dataModel.Transactions.Values.Where(t => _period.Includes(t.Timestamp)))
                RegisterTran(tran);
            return _balanceWithTurnovers.Balance();
        }

        private void RegisterTran(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    {
                        var myAcc = (AccountItemModel)tran.MyAccount.IsC(_accountItemModel);
                        if (myAcc != null)
                            _balanceWithTurnovers.Add(myAcc, tran.Currency, tran.Amount);
                    }
                    break;
                case OperationType.Расход:
                    {
                        var myAcc = (AccountItemModel)tran.MyAccount.IsC(_accountItemModel);
                        if (myAcc != null)
                            _balanceWithTurnovers.Sub(myAcc, tran.Currency, tran.Amount);
                    }
                    break;
                case OperationType.Перенос:
                    {
                        var myAcc = (AccountItemModel)tran.MyAccount.IsC(_accountItemModel);
                        if (myAcc != null)
                            _balanceWithTurnovers.Sub(myAcc, tran.Currency, tran.Amount);

                        var myAcc2 = (AccountItemModel)tran.MySecondAccount.IsC(_accountItemModel);
                        if (myAcc2 != null)
                            _balanceWithTurnovers.Add(myAcc2, tran.Currency, tran.Amount);
                    }
                    break;
                case OperationType.Обмен:
                    {
                        var myAcc = (AccountItemModel)tran.MyAccount.IsC(_accountItemModel);
                        if (myAcc != null)
                            _balanceWithTurnovers.Sub(myAcc, tran.Currency, tran.Amount);

                        var myAcc2 = (AccountItemModel)tran.MySecondAccount.IsC(_accountItemModel);
                        if (myAcc2 != null)
                            // ReSharper disable once PossibleInvalidOperationException
                            _balanceWithTurnovers.Add(myAcc2, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    }
                    break;
            }
        }

        public IEnumerable<KeyValuePair<DateTime, ListLine>> ColoredReport(BalanceOrTraffic mode)
        {
            return _balanceWithTurnovers.ColoredReport(mode);
        }

        public ListOfLines ReportForMonthAnalysis()
        {
            var result = new ListOfLines();

            var root = _balanceWithTurnovers.Summarize().Balance();
            result.AddList(_dataModel.BalanceReport(_period.FinishMoment, root, true));

            foreach (var pair in _balanceWithTurnovers.ChildAccounts)
            {
                result.Add("");
                result.Add($"   {pair.Key.Name}");
                var child = pair.Value.Balance();
                result.AddList(_dataModel.BalanceReport(_period.FinishMoment, child, false));
            }

            result.AddList(ReportInvestmentAssets());

            return result;
        }

        private ListOfLines ReportInvestmentAssets()
        {
            var result = new ListOfLines();
            result.Add("");
            result.Add("   Акции 2");

            decimal total = 0;
            foreach (var trustAccount in _dataModel.TrustAccounts)
            {
                var bal = _dataModel.GetBalancesOfEachAssetOfAccount(trustAccount, _period.FinishMoment);
                var line = _dataModel.AmountInUsdString(_period.FinishMoment, trustAccount.Currency, bal.Balance, out decimal usd);
                result.Add($"   {line}");
                total += usd;
            }

            result.Add($"   Итого {total:N} usd", FontWeights.SemiBold);

            return result;
        }
    }
}