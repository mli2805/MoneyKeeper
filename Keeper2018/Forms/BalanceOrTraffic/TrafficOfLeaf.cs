﻿using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrafficOfLeaf : ITraffic
    {
        private readonly AccountModel _accountModel;
        private readonly KeeperDb _db;
        private readonly TrafficLines _traffic = new TrafficLines();
        private readonly List<string> _shortTrans = new List<string>();

        public TrafficOfLeaf(AccountModel accountModel, KeeperDb db)
        {
            _accountModel = accountModel;
            _db = db;
        }

        public void RegisterTran(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    if (tran.MyAccount.Equals(_accountModel))
                    {
                        _traffic.Add(tran.Currency, tran.Amount);
                        _shortTrans.Add(ShortLine(tran, false, ""));
                    }
                    break;
                case OperationType.Расход:
                    if (tran.MyAccount.Equals(_accountModel))
                    {
                        _traffic.Sub(tran.Currency, tran.Amount);
                        _shortTrans.Add(ShortLine(tran, false, "-"));
                    }
                    break;
                case OperationType.Перенос:
                    if (tran.MyAccount.Equals(_accountModel))
                    {
                        _traffic.Sub(tran.Currency, tran.Amount);
                        _shortTrans.Add(ShortLine(tran, false, "-"));
                    }
                    if (tran.MySecondAccount.Equals(_accountModel))
                    {
                        _traffic.Add(tran.Currency, tran.Amount);
                        _shortTrans.Add(ShortLine(tran, false, ""));
                    }
                    break;
                case OperationType.Обмен:
                    if (tran.MyAccount.Equals(_accountModel))
                    {
                        _traffic.Sub(tran.Currency, tran.Amount);
                        _shortTrans.Add(ShortLine(tran, false, "-"));
                    }
                    if (tran.MySecondAccount.Equals(_accountModel))
                    {
                        // ReSharper disable once PossibleInvalidOperationException
                        _traffic.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        _shortTrans.Add(ShortLine(tran, true, ""));
                    }
                    break;
            }
        }

        public IEnumerable<string> Report()
        {
            foreach (var line in _traffic.Report())
            {
                yield return line;
            }
            foreach (var tran in _shortTrans)
            {
                yield return tran;
            }
        }

        private string ShortLine(TransactionModel tran, bool isInReturn, string sign)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {sign}{amount} {currency.ToString().ToLower()}";
            if (currency != CurrencyCode.USD)
            {
                var rateLine = _db.Bin.OfficialRates.First(r => r.Date.Date == tran.Timestamp.Date);
                var amountInUsd = currency == CurrencyCode.BYR || currency == CurrencyCode.BYN
                    ? (double)amount / rateLine.MyUsdRate.Value
                    : currency == CurrencyCode.EUR
                        ? (double)amount * rateLine.NbRates.Euro.Value / rateLine.NbRates.Usd.Value
                        : (double)amount * rateLine.NbRates.Rur.Value / rateLine.NbRates.Rur.Unit / rateLine.NbRates.Usd.Value;

                shortLine = shortLine + $" ({sign}{amountInUsd:#.00}$)";
            }
            return $"  {shortLine} {tran.Comment}";
        }
    }
}