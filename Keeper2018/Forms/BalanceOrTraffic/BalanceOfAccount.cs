using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class BalanceOfAccount
    {
        private readonly BalanceDictionary _balance = new BalanceDictionary();
        private readonly AccountModel _accountModel;
        private readonly KeeperDb _db;

        public BalanceOfAccount(AccountModel accountModel, KeeperDb db)
        {
            _accountModel = accountModel;
            _db = db;
        }

        public void RegisterTran(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _balance.Add(myAcc, tran.Currency, tran.Amount);
                        break;
                    }
                case OperationType.Расход:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _balance.Sub(myAcc, tran.Currency, tran.Amount);
                        break;
                    }
                case OperationType.Перенос:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _balance.Sub(myAcc, tran.Currency, tran.Amount);
                        var myAcc2 = tran.MySecondAccount.IsC(_accountModel);
                        if (myAcc2 != null)
                            _balance.Add(myAcc2, tran.Currency, tran.Amount);
                        break;
                    }
                case OperationType.Обмен:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _balance.Sub(myAcc, tran.Currency, tran.Amount);
                        var myAcc2 = tran.MySecondAccount.IsC(_accountModel);
                        if (myAcc2 != null && tran.CurrencyInReturn != null)
                            _balance.Add(myAcc2, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        break;
                    }
            }
        }

        public IEnumerable<string> Report(DateTime date)
        {
            var total = 0.0;
            foreach (var currency in _balance.Summarize().Currencies)
                if (currency.Value > 0)
                {
                    var amountInUsd = currency.Key == CurrencyCode.USD 
                                         ? (double)currency.Value 
                                         : _db.AmountInUsd(date, currency.Key, currency.Value);
                    total = total + amountInUsd;
                    yield return $"{currency.Key} {currency.Value:#,0.##}";
                }

            if (_accountModel.IsFolder)
                foreach (var str in _balance.Report())
                    yield return str;

            Total = $"{total:#,0.00}";
        }

        public string Total;
    }
}