using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
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

        public string GetAmountInUsd(DomainModel.Transactions.TranWithTags tranInWork)
        {
            return tranInWork.Currency != null ? 
                tranInWork.Currency == CurrencyCodes.USD ? "" :
                _rateExtractor.GetUsdEquivalentString(tranInWork.Amount, (CurrencyCodes)tranInWork.Currency, tranInWork.Timestamp) : "не задана валюта";
        }

        public string GetMyAccountBalance(DomainModel.Transactions.TranWithTags transactionInWork)
        {
            if (transactionInWork == null || transactionInWork.MyAccount == null || !transactionInWork.MyAccount.Is("Мои")) return "было ххх - стало ххх";

            var balanceBefore =
                _db.TransWithTags.Sum(a => a.AmountForAccount(transactionInWork.MyAccount, transactionInWork.Currency, transactionInWork.Timestamp.AddMilliseconds(-1)));

            return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(transactionInWork.MyAccount,transactionInWork.Currency), transactionInWork.Currency);
        }
    }
}
