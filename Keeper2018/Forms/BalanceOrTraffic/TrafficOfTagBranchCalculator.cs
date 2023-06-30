using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class TrafficOfTagBranchCalculator : ITraffic
    {
        private readonly KeeperDataModel _dataModel;
        private readonly AccountItemModel _tag;
        private readonly Period _period;

        private readonly BalanceWithTurnoverOfBranch _balanceWithTurnovers = new BalanceWithTurnoverOfBranch();
        private readonly TrafficPair _trafficInUsd = new TrafficPair();
        public string Total => $"{_trafficInUsd.Plus:#,0.##} - {Math.Abs(_trafficInUsd.Minus):#,0.##} = {_trafficInUsd.Plus + _trafficInUsd.Minus:#,0.##;- #,0.##} usd ( знак относительно меня)";
     //   public string Total => "";

        public TrafficOfTagBranchCalculator(KeeperDataModel dataModel, AccountItemModel tag, Period period)
        {
            _dataModel = dataModel;
            _tag = tag;
            _period = period;
        }

        public void EvaluateAccount()
        {
            foreach (var tran in _dataModel.Transactions.Values.Where(t => _period.Includes(t.Timestamp)))
            {
                foreach (var tag in tran.Tags)
                {
                    var myTag = (AccountItemModel)tag.IsC(_tag);
                    if (myTag != null)
                        RegisterTran(tran, myTag);
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

        public IEnumerable<KeyValuePair<DateTime, string>> Report(BalanceOrTraffic mode)
        {
            return _balanceWithTurnovers.Report(mode);
        }
    }
}