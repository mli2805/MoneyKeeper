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
        private readonly SortedDictionary<DateTime, string> _shortTrans = new SortedDictionary<DateTime, string>();

        private readonly TrafficPair _trafficInUsd = new TrafficPair();
        public string Total => $"{_trafficInUsd.Plus:#,0.##} - {Math.Abs(_trafficInUsd.Minus):#,0.##} = {_trafficInUsd.Plus + _trafficInUsd.Minus:#,0.##;- #,0.##} usd ( знак относительно меня)";

        public TrafficOfTagCalculator(KeeperDb db, AccountModel accountModel, Period period)
        {
            _db = db;
            _accountModel = accountModel;
            _period = period;
        }

        public void Evaluate()
        {
            foreach (var tran in _db.Bin.Transactions.Values.Where(t => _period.Includes(t.Timestamp)))
            {
                if (!tran.Tags.Contains(_accountModel.Id)) continue;

                decimal inUsd;
                switch (tran.Operation)
                {
                    case OperationType.Доход:
                        _shortTrans.Add(tran.Timestamp, _db.ShortLine(tran, false, 1, out inUsd));
                        _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                        _balanceWithTurnover.Add(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Расход:
                        _shortTrans.Add(tran.Timestamp, _db.ShortLine(tran, false, -1, out inUsd));
                        _trafficInUsd.Minus = _trafficInUsd.Minus + inUsd;
                        _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Перенос:
                        _balanceWithTurnover.Add(tran.Currency, tran.Amount);
                        _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Обмен:
                        _shortTrans.Add(tran.Timestamp, _db.ShortLineOneAccountExchange(tran));
                        // я меняю в банке    15000 дол     на    31000 бел
                        //                 Amount/Currency      AmountInReturn/CurrencyInReturn
                        // для меня 15000 дол идут в минус 31000 бел идет в плюс 
                        // итоговый знак должен показывать прибыльность операции для меня а не для банка (tag)
                        inUsd = _db.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                        _trafficInUsd.Minus = _trafficInUsd.Minus - inUsd;
                        _balanceWithTurnover.Sub(tran.Currency, tran.Amount);

                        inUsd = _db.AmountInUsd(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn);
                        _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                        // ReSharper disable once PossibleInvalidOperationException
                        _balanceWithTurnover.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
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
            foreach (var pair in _shortTrans.Reverse())
            {
                yield return pair.Value;
            }
        }
    }
}