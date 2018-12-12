using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrafficOfTagCalculator : ITraffic
    {
        private readonly KeeperDb _db;
        private readonly AccountModel _accountModel;
        private readonly Period _period;
        private readonly BalanceWithTurnover _balanceWithTurnover = new BalanceWithTurnover();
        private readonly List<string> _shortTrans = new List<string>();

        private readonly TrafficPair _trafficInUsd = new TrafficPair();
        public string Total => $"{_trafficInUsd.Plus:#,0.##} - {Math.Abs(_trafficInUsd.Minus):#,0.##} = {_trafficInUsd.Plus + _trafficInUsd.Minus:#,0.##} usd";

        public TrafficOfTagCalculator(KeeperDb db, AccountModel accountModel, Period period)
        {
            _db = db;
            _accountModel = accountModel;
            _period = period;
        }

        public void Evaluate()
        {
            foreach (var tran in _db.TransactionModels.Where(t => _period.Includes(t.Timestamp)))
            {
                if (!tran.Tags.Contains(_accountModel)) continue;

                decimal inUsd;
                switch (tran.Operation)
                {
                    case OperationType.Доход:
                        _shortTrans.Add(_db.ShortLine(tran, false, 1, out inUsd));
                        _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                        _balanceWithTurnover.Add(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Расход:
                        _shortTrans.Add(_db.ShortLine(tran, false, -1, out inUsd));
                        _trafficInUsd.Minus = _trafficInUsd.Minus + inUsd;
                        _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Перенос:
                        inUsd = _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                        _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                        _trafficInUsd.Minus = _trafficInUsd.Minus + inUsd;
                        _balanceWithTurnover.Add(tran.Currency, tran.Amount);
                        _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Обмен:
                        inUsd = _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                        _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                        _trafficInUsd.Minus = _trafficInUsd.Minus + inUsd;
                        _balanceWithTurnover.Add(tran.Currency, tran.Amount);
                        // ReSharper disable once PossibleInvalidOperationException
                        _balanceWithTurnover.Sub((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        break;
                }
            }
        }

        public IEnumerable<string> Report(BalanceOrTraffic mode)
        {
            foreach (var line in _balanceWithTurnover.Report(mode))
            {
                yield return line;
            }
            foreach (var tran in _shortTrans)
            {
                yield return tran;
            }
        }
    }
}