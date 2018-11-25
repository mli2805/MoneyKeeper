using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class BalanceOfAccount
    {
        public readonly BalanceDictionary Balance = new BalanceDictionary();
        private readonly KeeperDb _db;
        private readonly AccountModel _accountModel;
        private readonly Period _period;
        public List<string> Report;
        public decimal AmountInUsd;
        public string Total;

        public BalanceOfAccount(KeeperDb db, AccountModel accountModel, Period period)
        {
            _accountModel = accountModel;
            _period = period;
            _db = db;
        }

        public void Evaluate()
        {
            foreach (var tran in _db.TransactionModels.Where(t => _period.Includes(t.Timestamp)))
                RegisterTran(tran);
            CreateReportAndTotal();
        }

        private void RegisterTran(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            Balance.Add(myAcc, tran.Currency, tran.Amount);
                        break;
                    }
                case OperationType.Расход:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            Balance.Sub(myAcc, tran.Currency, tran.Amount);
                        break;
                    }
                case OperationType.Перенос:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            Balance.Sub(myAcc, tran.Currency, tran.Amount);
                        var myAcc2 = tran.MySecondAccount.IsC(_accountModel);
                        if (myAcc2 != null)
                            Balance.Add(myAcc2, tran.Currency, tran.Amount);
                        break;
                    }
                case OperationType.Обмен:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            Balance.Sub(myAcc, tran.Currency, tran.Amount);
                        var myAcc2 = tran.MySecondAccount.IsC(_accountModel);
                        if (myAcc2 != null && tran.CurrencyInReturn != null)
                            Balance.Add(myAcc2, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                        break;
                    }
            }
        }

        private void CreateReportAndTotal()
        {
            Report = new List<string>();
            AmountInUsd = 0;
            foreach (var currency in Balance.Summarize().Currencies)
                if (currency.Value > 0)
                {
                    var amountInUsd = currency.Key == CurrencyCode.USD
                                         ? currency.Value
                                         : _db.AmountInUsd(_period.FinishMoment, currency.Key, currency.Value);
                    AmountInUsd = AmountInUsd + amountInUsd;
                    Report.Add($"{currency.Key} {currency.Value:#,0.##}");
                }

            if (_accountModel.IsFolder)
                foreach (var str in Balance.Report())
                    Report.Add(str);

            Total = $"{AmountInUsd:#,0.00}";
        }
    }
}