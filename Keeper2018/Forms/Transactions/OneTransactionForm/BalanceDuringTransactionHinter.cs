using System;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class BalanceDuringTransactionHinter
    {
        private const string TemplateForByr = "{0:#,0} {2} -> {1:#,0} {2}";
        private const string TemplateForCurrencies = "{0:#,0.00} {2} -> {1:#,0.00} {2}";
        private readonly KeeperDataModel _dataModel;

        public BalanceDuringTransactionHinter(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
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
                : _dataModel.AmountInUsdString(tranInWork.Timestamp, tranInWork.Currency, tranInWork.Amount);
        }

        public string GetAmountInReturnInUsd(TransactionModel tranInWork)
        {
            return tranInWork.CurrencyInReturn != null
                    ? tranInWork.CurrencyInReturn == CurrencyCode.USD
                        ? ""
                        : _dataModel.AmountInUsdString(tranInWork.Timestamp, tranInWork.CurrencyInReturn, tranInWork.AmountInReturn)
                    : "не задана валюта";
        }


        public string GetMyAccountBalance(TransactionModel transactionInWork)
        {
            if (transactionInWork?.MyAccount == null || !transactionInWork.MyAccount.Is(158)) return "было ххх - стало ххх";

            var balanceBefore =
                _dataModel.Transactions.Values.Sum(t => t.AmountForAccount(
                    _dataModel, transactionInWork.MyAccount, transactionInWork.Currency, transactionInWork.Timestamp.AddMilliseconds(-1)));

            return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(
                                               transactionInWork.MyAccount, transactionInWork.Currency), transactionInWork.Currency);
        }

        public string GetMySecondAccountBalance(TransactionModel transactionInWork)
        {
            if (transactionInWork?.MySecondAccount == null) return "было ххх - стало ххх";
            if (!transactionInWork.MySecondAccount.Is(158)) return "было ххх - стало ххх";

            if (transactionInWork.Operation == OperationType.Перенос)
            {
                var balanceBefore =
                    _dataModel.Transactions.Values.Sum(a => a.AmountForAccount(
                        _dataModel, transactionInWork.MySecondAccount, transactionInWork.Currency, transactionInWork.Timestamp.AddMilliseconds(-1)));

                return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(
                                                   transactionInWork.MySecondAccount, transactionInWork.Currency), transactionInWork.Currency);
            }
            else // OperationType.Обмен
            {
                var balanceBefore =
                    _dataModel.Transactions.Values.Sum(a => a.AmountForAccount(
                        _dataModel, transactionInWork.MySecondAccount, transactionInWork.CurrencyInReturn, transactionInWork.Timestamp.AddMilliseconds(-1)));

                return BuildTip(balanceBefore, balanceBefore +
                                  transactionInWork.AmountForAccount(transactionInWork.MySecondAccount, transactionInWork.CurrencyInReturn),
                                                transactionInWork.CurrencyInReturn);

            }
        }

        public string GetExchangeRate(TransactionModel transactionInWork)
        {
            if (transactionInWork.Amount.Equals(0) || transactionInWork.AmountInReturn.Equals(0)) return "";
            return transactionInWork.Amount > transactionInWork.AmountInReturn
                ? $"Курс обмена {transactionInWork.Amount / transactionInWork.AmountInReturn: #,0.00###} {transactionInWork.Currency.ToString().ToLower()}/{transactionInWork.CurrencyInReturn.ToString().ToLower()}"
                : $"Курс обмена {transactionInWork.AmountInReturn / transactionInWork.Amount: #,0.00###} {transactionInWork.CurrencyInReturn.ToString().ToLower()}/{transactionInWork.Currency.ToString().ToLower()}";
        }
    }
}
