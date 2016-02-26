using System;
using System.Composition;
using Keeper.ByFunctional.BalanceEvaluating;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;
using Keeper.Utils.Rates;

namespace Keeper.ViewModels.Transactions
{
    [Export]
    class BalanceDuringTransactionHinter
    {
        private const string TemplateForByr = "{0:#,0} {2} -> {1:#,0} {2}";
        private const string TemplateForCurrencies = "{0:#,0.00} {2} -> {1:#,0.00} {2}";
        private readonly AccountBalanceCalculator _accountBalanceCalculator;
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public BalanceDuringTransactionHinter(AccountBalanceCalculator accountBalanceCalculator, RateExtractor rateExtractor)
        {
            _accountBalanceCalculator = accountBalanceCalculator;
            _rateExtractor = rateExtractor;
        }

        private string BuildTip(decimal before, decimal after, CurrencyCodes? currency)
        {
            return currency == CurrencyCodes.BYR
                ? String.Format(TemplateForByr, before, after, currency.ToString().ToLower())
                : String.Format(TemplateForCurrencies, before, after, currency.ToString().ToLower());
        }

        public string GetAmountInUsd(Transaction transactionInWork, Transaction relatedTransactionInWork, int selectedTabIndex)
        {
            // одинарные операции не долларах
            if (transactionInWork.Currency == CurrencyCodes.USD && selectedTabIndex != 3) return "";
            const string res0 = "                                                                                ";

            var res1 = _rateExtractor.GetUsdEquivalentString(transactionInWork.Amount, transactionInWork.Currency,
                transactionInWork.Timestamp);
            // одинарные операции не в остальных валютах
            if (selectedTabIndex != 3) return res0 + res1;

            res1 = transactionInWork.Currency == CurrencyCodes.USD
                ? "                                           "
                : _rateExtractor.GetUsdEquivalentString(transactionInWork.Amount, transactionInWork.Currency,
                    transactionInWork.Timestamp);
            var res2 = relatedTransactionInWork.Currency == CurrencyCodes.USD
                ? ""
                : _rateExtractor.GetUsdEquivalentString(relatedTransactionInWork.Amount, relatedTransactionInWork.Currency,
                    relatedTransactionInWork.Timestamp);

            return res1 + "                                      " + res2;
        }

        public string GetMyAccountBalance(TranWithTags transactionInWork)
        {
            if (transactionInWork == null || transactionInWork.MyAccount == null || !transactionInWork.MyAccount.Is("Мои")) return "было ххх - стало ххх";

            var periodBefore = new Period(new DateTime(0), transactionInWork.Timestamp.AddMilliseconds(-1));
            var balanceBefore = _accountBalanceCalculator.GetMyAccountBalanceForCurrency(
                                  transactionInWork.MyAccount, periodBefore, transactionInWork.Currency);

            return BuildTip(balanceBefore, balanceBefore + transactionInWork.AmountForAccount(transactionInWork.MyAccount,transactionInWork.Currency), transactionInWork.Currency);
        }
    }
}
