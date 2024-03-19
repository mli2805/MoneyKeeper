using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class TrafficOfTagCalculator : ITraffic
    {
        private readonly KeeperDataModel _dataModel;
        private readonly AccountItemModel _accountItemModel;
        private readonly Period _period;
        private readonly BalanceWithTurnover _balanceWithTurnover = new BalanceWithTurnover();
        private readonly SortedDictionary<DateTime, ListLine> _coloredTrans = new SortedDictionary<DateTime, ListLine>();

        private readonly TrafficPair _trafficInUsd = new TrafficPair();
        public string Total => $"{_trafficInUsd.Plus:#,0.##} - {Math.Abs(_trafficInUsd.Minus):#,0.##} = {_trafficInUsd.Plus + _trafficInUsd.Minus:#,0.##;- #,0.##} usd ( знак относительно меня)";

        public TrafficOfTagCalculator(KeeperDataModel dataModel, AccountItemModel accountItemModel, Period period)
        {
            _dataModel = dataModel;
            _accountItemModel = accountItemModel;
            _period = period;
        }

        public void EvaluateAccount()
        {
            foreach (var tran in _dataModel.Transactions.Values.Where(t => _period.Includes(t.Timestamp)))
            {
                if (!tran.Tags.Select(t=>t.Id).Contains(_accountItemModel.Id)) continue;

                decimal inUsd;
                switch (tran.Operation)
                {
                    case OperationType.Доход:
                        _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLine(tran, false, 1, out inUsd));
                        _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                        _balanceWithTurnover.Add(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Расход:
                        _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLine(tran, false, -1, out inUsd));
                        _trafficInUsd.Minus = _trafficInUsd.Minus + inUsd;
                        _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Перенос:
                        _balanceWithTurnover.Add(tran.Currency, tran.Amount);
                        _balanceWithTurnover.Sub(tran.Currency, tran.Amount);
                        break;
                    case OperationType.Обмен:
                        _coloredTrans.Add(tran.Timestamp, _dataModel.ColoredLineOneAccountExchange(tran));
                        // я меняю в банке    15000 дол     на    31000 бел
                        //                 Amount/Currency      AmountInReturn/CurrencyInReturn
                        // для меня 15000 дол идут в минус 31000 бел идет в плюс 
                        // итоговый знак должен показывать прибыльность операции для меня а не для банка (tag)
                        inUsd = _dataModel.AmountInUsd(tran.Timestamp, tran.Currency, tran.Amount);
                        _trafficInUsd.Minus = _trafficInUsd.Minus - inUsd;
                        _balanceWithTurnover.Sub(tran.Currency, tran.Amount);

                        inUsd = _dataModel.AmountInUsd(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn);
                        _trafficInUsd.Plus = _trafficInUsd.Plus + inUsd;
                        // ReSharper disable once PossibleInvalidOperationException
                        _balanceWithTurnover.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        break;
                }
            }
        }

        public IEnumerable<KeyValuePair<DateTime, ListLine>> ColoredReport(BalanceOrTraffic mode)
        {
            foreach (var line in _balanceWithTurnover.ColoredReport(mode))
            {
                yield return line;
            }
            foreach (var pair in _coloredTrans.Reverse())
            {
                yield return pair;
            }
        }
    }
}