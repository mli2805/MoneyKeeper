using System.Collections.Generic;

namespace Keeper2018
{
    public class TrafficOfFolder : ITraffic
    {
        private readonly AccountModel _accountModel;
        private readonly TrafficDictionaries _traffics = new TrafficDictionaries();

        public TrafficOfFolder(AccountModel accountModel)
        {
            _accountModel = accountModel;
        }

        public void RegisterTran(TransactionModel tran)
        {
            switch (tran.Operation)
            {
                case OperationType.Доход:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _traffics.Add(myAcc, tran.Currency, tran.Amount);
                    }
                    break;
                case OperationType.Расход:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _traffics.Sub(myAcc, tran.Currency, tran.Amount);
                    }
                    break;
                case OperationType.Перенос:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _traffics.Sub(myAcc, tran.Currency, tran.Amount);
                        var myAcc2 = tran.MySecondAccount.IsC(_accountModel);
                        if (myAcc2 != null)
                            _traffics.Add(myAcc2, tran.Currency, tran.Amount);
                    }
                    break;
                case OperationType.Обмен:
                    {
                        var myAcc = tran.MyAccount.IsC(_accountModel);
                        if (myAcc != null)
                            _traffics.Sub(myAcc, tran.Currency, tran.Amount);
                        var myAcc2 = tran.MySecondAccount.IsC(_accountModel);
                        if (myAcc2 != null)
                            // ReSharper disable once PossibleInvalidOperationException
                            _traffics.Add(myAcc2, (CurrencyCode)tran.CurrencyInReturn, tran.AmountInReturn);
                    }
                    break;
            }
        }

        public IEnumerable<string> Report() { return _traffics.Report(); }
    }
}