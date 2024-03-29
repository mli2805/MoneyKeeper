using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class TrafficOfTagBranchCalculator : ITraffic
    {
        private readonly KeeperDataModel _dataModel;
        private readonly AccountItemModel _accountItemModel;
        private readonly Period _period;

        private readonly BalanceWithTurnoverOfBranch _balanceWithTurnovers = new BalanceWithTurnoverOfBranch();
        private readonly TrafficPair _trafficInUsd = new TrafficPair();
        public string Total => $"{_trafficInUsd.Plus:#,0.##} - {Math.Abs(_trafficInUsd.Minus):#,0.##} = {_trafficInUsd.Plus + _trafficInUsd.Minus:#,0.##;- #,0.##} usd ( знак относительно меня)";

        public TrafficOfTagBranchCalculator(KeeperDataModel dataModel, AccountItemModel accountItemModel, Period period)
        {
            _dataModel = dataModel;
            _accountItemModel = accountItemModel;
            _period = period;
        }

        public void EvaluateAccount()
        {
            var isCategory = _accountItemModel.IsCategory();
            var isCounterparty = _accountItemModel.IsCounterparty();

            foreach (var tran in _dataModel.Transactions.Values.Where(t => _period.Includes(t.Timestamp)))
            {
                if (isCategory)
                {
                    if (tran.Category != null && tran.Category.Is(_accountItemModel)) 
                        RegisterTran(tran, tran.Category);
                }
                else if (isCounterparty)
                {
                    if (tran.Counterparty != null && tran.Counterparty.Is(_accountItemModel))
                        RegisterTran(tran, tran.Counterparty);
                }
                else
                {
                    foreach (var tag in tran.Tags)
                    {
                        if (tag.Is(_accountItemModel))
                            RegisterTran(tran, tag);
                    }
                }
            }
        }

        private void RegisterTran(TransactionModel tran, AccountItemModel myTag)
        {
                decimal inUsd;
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    _balanceWithTurnovers.Add(myTag, tran.Currency, tran.Amount);
                    inUsd = _dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                    _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                    break;
                case OperationType.Расход:
                    _balanceWithTurnovers.Sub(myTag, tran.Currency, tran.Amount);
                    inUsd = _dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                    _trafficInUsd.Minus = _trafficInUsd.Minus - inUsd;
                    break;
                case OperationType.Перенос:
                    _balanceWithTurnovers.Add(myTag, tran.Currency, tran.Amount);
                    _balanceWithTurnovers.Sub(myTag, tran.Currency, tran.Amount);
                    break;
                case OperationType.Обмен:
                    _balanceWithTurnovers.Add(myTag, tran.Currency, tran.Amount);
                    // ReSharper disable once PossibleInvalidOperationException
                    _balanceWithTurnovers.Sub(myTag, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);

                        inUsd = _dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                        _trafficInUsd.Minus = _trafficInUsd.Minus - inUsd;
                        inUsd = _dataModel.AmountInUsd(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn);
                        _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                    break;
            }
        }

        public IEnumerable<KeyValuePair<DateTime, ListLine>> ColoredReport(BalanceOrTraffic mode)
        {
            return _balanceWithTurnovers.ColoredReport(mode);
        }
    }
}