using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Trans;
using Keeper.Utils.Rates;

namespace Keeper.ViewModels.TransWithTags
{
    [Export]
    class BalanceDuringTransactionHinter
    {
        private const string TemplateForByr = "{0:#,0} {2} -> {1:#,0} {2}";
        private const string TemplateForCurrencies = "{0:#,0.00} {2} -> {1:#,0.00} {2}";
        private readonly KeeperDb _db;
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public BalanceDuringTransactionHinter(KeeperDb db, RateExtractor rateExtractor)
        {
            _db = db;
            _rateExtractor = rateExtractor;
        }

        private string BuildTip(decimal before, decimal after, CurrencyCodes? currency)
        {
            return currency == CurrencyCodes.BYR
                ? String.Format(TemplateForByr, before, after, currency.ToString().ToLower())
                : String.Format(TemplateForCurrencies, before, after, currency.ToString().ToLower());
        }

        public string GetAmountInUsd(TranWithTags tranInWork)
        {
            return tranInWork.Currency != null ?
                tranInWork.Currency == CurrencyCodes.USD ? "" :
                _rateExtractor.GetUsdEquivalentString(tranInWork.Amount, (CurrencyCodes)tranInWork.Currency, tranInWork.Timestamp) : "не задана валюта";
        }

        public string GetAmountInReturnInUsd(TranWithTags tranInWork)
        {
            return tranInWork.CurrencyInReturn != null ?
                tranInWork.CurrencyInReturn == CurrencyCodes.USD ? "" :
                _rateExtractor.GetUsdEquivalentString(tranInWork.AmountInReturn, (CurrencyCodes)tranInWork.CurrencyInReturn, tranInWork.Timestamp) : "не задана валюта";
        }

        public string GetMyAccountBalance(TranWithTags transactionInWork)
        {
            if (transactionInWork?.MyAccount == null || !transactionInWork.MyAccount.Is("Мои")) return "было ххх - стало ххх";

            var balanceBefore =
                _db.TransWithTags.Sum(a => a.AmountForAccount(transactionInWork.MyAccount, transactionInWork.Currency, transactionInWork.Timestamp.AddMilliseconds(-1)));

            return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(transactionInWork.MyAccount, transactionInWork.Currency), transactionInWork.Currency);
        }

        public string GetMySecondAccountBalance(TranWithTags transactionInWork)
        {
            if (transactionInWork == null || transactionInWork.MySecondAccount == null || !transactionInWork.MySecondAccount.Is("Мои")) return "было ххх - стало ххх";

            if (transactionInWork.Operation == OperationType.Перенос)
            {
                var balanceBefore =
                    _db.TransWithTags.Sum(a => a.AmountForAccount(transactionInWork.MySecondAccount, transactionInWork.Currency, transactionInWork.Timestamp.AddMilliseconds(-1)));

                return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(transactionInWork.MySecondAccount, transactionInWork.Currency), transactionInWork.Currency);
            }
            else // OperationType.Обмен
            {
                var balanceBefore =
                    _db.TransWithTags.Sum(a => a.AmountForAccount(transactionInWork.MySecondAccount, transactionInWork.CurrencyInReturn, transactionInWork.Timestamp.AddMilliseconds(-1)));

                return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(transactionInWork.MySecondAccount, transactionInWork.CurrencyInReturn), transactionInWork.CurrencyInReturn);

            }
        }

        public string GetExchangeRate(TranWithTags transactionInWork)
        {
            if (transactionInWork.Amount.Equals(0) || transactionInWork.AmountInReturn.Equals(0)) return "";
            return transactionInWork.Amount > transactionInWork.AmountInReturn 
                ? $"Курс обмена {transactionInWork.Amount / transactionInWork.AmountInReturn : #,0.00###} {transactionInWork.Currency.ToString().ToLower()}/{transactionInWork.CurrencyInReturn.ToString().ToLower()}" 
                : $"Курс обмена {transactionInWork.AmountInReturn / transactionInWork.Amount : #,0.00###} {transactionInWork.CurrencyInReturn.ToString().ToLower()}/{transactionInWork.Currency.ToString().ToLower()}";
        }
    }
}
