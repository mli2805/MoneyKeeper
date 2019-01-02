using System;
using System.Linq;

namespace Keeper2018
{
    public class BalanceDuringTransactionHinter
    {
        private const string TemplateForByr = "{0:#,0} {2} -> {1:#,0} {2}";
        private const string TemplateForCurrencies = "{0:#,0.00} {2} -> {1:#,0.00} {2}";
        private readonly KeeperDb _db;

        public BalanceDuringTransactionHinter(KeeperDb db)
        {
            _db = db;
        }

        private string BuildTip(decimal before, decimal after, CurrencyCode? currency)
        {
            return currency == CurrencyCode.BYR
                ? String.Format(TemplateForByr, before, after, currency.ToString().ToLower())
                : String.Format(TemplateForCurrencies, before, after, currency.ToString().ToLower());
        }

        public string GetAmountInUsd(TransactionModel tranInWork)
        {
            return tranInWork.Currency == CurrencyCode.USD
                ? ""
                : _db.AmountInUsdString(tranInWork.Timestamp, tranInWork.Currency, tranInWork.Amount);
        }

        public string GetAmountInReturnInUsd(TransactionModel tranInWork)
        {
            return tranInWork.CurrencyInReturn != null 
                    ? tranInWork.CurrencyInReturn == CurrencyCode.USD 
                        ? "" 
                        : _db.AmountInUsdString(tranInWork.Timestamp, tranInWork.CurrencyInReturn, tranInWork.AmountInReturn)
                    : "не задана валюта";
        }

        public string GetMyAccountBalance(Transaction transactionInWork)
        {
            if (transactionInWork == null || transactionInWork.MyAccount == 0) return "было ххх - стало ххх";
            var accModel = _db.AcMoDict[transactionInWork.MyAccount];
            if (!accModel.Is("Мои")) return "было ххх - стало ххх";

            var balanceBefore =
                _db.Bin.Transactions.Values.Sum(a => a.AmountForAccount(_db, transactionInWork.MyAccount, transactionInWork.Currency, transactionInWork.Timestamp.AddMilliseconds(-1)));

            return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(_db, transactionInWork.MyAccount, transactionInWork.Currency), transactionInWork.Currency);
        }

        public string GetMySecondAccountBalance(Transaction transactionInWork)
        {
            if (transactionInWork == null || transactionInWork.MySecondAccount == 0) return "было ххх - стало ххх";
            var secondAccModel = _db.AcMoDict[transactionInWork.MySecondAccount];
            if (!secondAccModel.Is("Мои")) return "было ххх - стало ххх";

            if (transactionInWork.Operation == OperationType.Перенос)
            {
                var balanceBefore =
                    _db.Bin.Transactions.Values.Sum(a => a.AmountForAccount(_db, transactionInWork.MySecondAccount, transactionInWork.Currency, transactionInWork.Timestamp.AddMilliseconds(-1)));

                return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(_db, transactionInWork.MySecondAccount, transactionInWork.Currency), transactionInWork.Currency);
            }
            else // OperationType.Обмен
            {
                var balanceBefore =
                    _db.Bin.Transactions.Values.Sum(a => a.AmountForAccount(_db, transactionInWork.MySecondAccount, transactionInWork.CurrencyInReturn, transactionInWork.Timestamp.AddMilliseconds(-1)));

                return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(_db, transactionInWork.MySecondAccount, transactionInWork.CurrencyInReturn), transactionInWork.CurrencyInReturn);

            }
        }

        public string GetExchangeRate(TransactionModel transactionInWork)
        {
            if (transactionInWork.Amount.Equals(0) || transactionInWork.AmountInReturn.Equals(0)) return "";
            return transactionInWork.Amount > transactionInWork.AmountInReturn 
                ? $"Курс обмена {transactionInWork.Amount / transactionInWork.AmountInReturn : #,0.00###} {transactionInWork.Currency.ToString().ToLower()}/{transactionInWork.CurrencyInReturn.ToString().ToLower()}" 
                : $"Курс обмена {transactionInWork.AmountInReturn / transactionInWork.Amount : #,0.00###} {transactionInWork.CurrencyInReturn.ToString().ToLower()}/{transactionInWork.Currency.ToString().ToLower()}";
        }
    }
}
