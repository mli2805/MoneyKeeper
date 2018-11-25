using System.Collections.Generic;

namespace Keeper2018
{
    public class TrafficOfLeaf : ITraffic
    {
        private readonly AccountModel _accountModel;
        private readonly KeeperDb _db;
        private readonly TrafficLines _traffic = new TrafficLines();
        private readonly List<string> _shortTrans = new List<string>();
        public string Total { get; set; }

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
                        _shortTrans.Add(ShortLine(tran, false, 1));
                    }
                    break;
                case OperationType.Расход:
                    if (tran.MyAccount.Equals(_accountModel))
                    {
                        _traffic.Sub(tran.Currency, tran.Amount);
                        _shortTrans.Add(ShortLine(tran, false, -1));
                    }
                    break;
                case OperationType.Перенос:
                    if (tran.MyAccount.Equals(_accountModel))
                    {
                        _traffic.Sub(tran.Currency, tran.Amount);
                        _shortTrans.Add(ShortLine(tran, false, -1));
                    }
                    if (tran.MySecondAccount.Equals(_accountModel))
                    {
                        _traffic.Add(tran.Currency, tran.Amount);
                        _shortTrans.Add(ShortLine(tran, false, 1));
                    }
                    break;
                case OperationType.Обмен:
                    // явочный обмен - приходишь в банк и меняешь - была в кармане одна валюта - стала другая в том же кармане
                    if (tran.MyAccount.Equals(_accountModel) && tran.MySecondAccount.Equals(_accountModel))
                    {
                        _traffic.Sub(tran.Currency, tran.Amount);
                        // ReSharper disable once PossibleInvalidOperationException
                        _traffic.Add((CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        _shortTrans.Add(ShortLineOneAccountExchange(tran));
                    }
                    // безнальный обмен - с одного счета списалась одна валюта, на другой зачислилась другая
                    else
                    {
                        if (tran.MyAccount.Equals(_accountModel))
                        {
                            _traffic.Sub(tran.Currency, tran.Amount);
                            _shortTrans.Add(ShortLine(tran, false, -1));
                        }

                        if (tran.MySecondAccount.Equals(_accountModel))
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            _traffic.Add((CurrencyCode) tran.CurrencyInReturn, tran.AmountInReturn);
                            _shortTrans.Add(ShortLine(tran, true, 1));
                        }
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

        private string ShortLine(TransactionModel tran, bool isInReturn, int sign)
        {
            var amount = isInReturn ? tran.AmountInReturn : tran.Amount;
            var currency = isInReturn ? tran.CurrencyInReturn : tran.Currency;
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {_db.AmountInUsdString(tran.Timestamp, currency, amount * sign)}";
            return $"  {shortLine}   {tran.Comment}";
        }

        private string ShortLineOneAccountExchange(TransactionModel tran)
        {
            var minus = $"{_db.AmountInUsdString(tran.Timestamp, tran.Currency, tran.Amount * -1)}";
            var plus = $"{_db.AmountInUsdString(tran.Timestamp, tran.CurrencyInReturn, tran.AmountInReturn)}";
            var shortLine = $"{tran.Timestamp.Date.ToShortDateString()}  {minus} -> {plus}";
            return $"  {shortLine}   {tran.Comment}";
        }
    }
}