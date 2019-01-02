using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrafficOfTagBranchCalculator : ITraffic
    {
        private readonly KeeperDb _db;
        private readonly AccountModel _tag;
        private readonly Period _period;

        private readonly BalanceWithTurnoverOfBranch _balanceWithTurnovers = new BalanceWithTurnoverOfBranch();
        private readonly TrafficPair _trafficInUsd = new TrafficPair();
        public string Total => $"{_trafficInUsd.Plus:#,0.##} - {Math.Abs(_trafficInUsd.Minus):#,0.##} = {_trafficInUsd.Plus + _trafficInUsd.Minus:#,0.##;- #,0.##} usd ( знак относительно меня)";
     //   public string Total => "";

        public TrafficOfTagBranchCalculator(KeeperDb db, AccountModel tag, Period period)
        {
            _db = db;
            _tag = tag;
            _period = period;
        }

        public void Evaluate()
        {
            foreach (var tran in _db.Bin.Transactions.Values.Where(t => _period.Includes(t.Timestamp)))
            {
                foreach (var tag in tran.Tags)
                {
                    var tagAccModel = _db.AcMoDict[tag];
                    var myTag = tagAccModel.IsC(_tag);
                    if (myTag != null)
                        RegisterTran(tran, myTag);
                }
            }
        }

        private void RegisterTran(Transaction tran, AccountModel myTag)
        {
                decimal inUsd;
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    _balanceWithTurnovers.Add(myTag, tran.Currency, tran.Amount);
                    inUsd = _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                    _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                    break;
                case OperationType.Расход:
                    _balanceWithTurnovers.Sub(myTag, tran.Currency, tran.Amount);
                    inUsd = _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
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

                        inUsd = _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                        _trafficInUsd.Minus = _trafficInUsd.Minus - inUsd;
                        inUsd = _db.AmountInUsd(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn);
                        _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                    break;
            }
        }

        public IEnumerable<string> Report(BalanceOrTraffic mode) { return _balanceWithTurnovers.Report(mode); }
    }
}