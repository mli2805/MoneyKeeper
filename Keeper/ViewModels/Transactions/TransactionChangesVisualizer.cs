using System;
using System.Composition;
using Keeper.ByFunctional.BalanceEvaluating;
using Keeper.DomainModel;
using Keeper.DomainModel.Transactions;
using Keeper.Utils.Rates;

namespace Keeper.Utils.CommonKeeper
{
    [Export]
    public class TransactionChangesVisualizer
    {
        private const string TemplateForByr = "{0:#,0} {2} -> {1:#,0} {2}";
        private const string TemplateForCurrencies = "{0:#,0.00} {2} -> {1:#,0.00} {2}";
        private readonly AccountBalanceCalculator _accountBalanceCalculator;
        private readonly RateExtractor _rateExtractor;

        [ImportingConstructor]
        public TransactionChangesVisualizer(AccountBalanceCalculator accountBalanceCalculator, RateExtractor rateExtractor)
        {
            _accountBalanceCalculator = accountBalanceCalculator;
            _rateExtractor = rateExtractor;
        }

        private string BuildTip(decimal before, decimal after, CurrencyCodes currency)
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


        public string GetDebetAccountBalance(Transaction transactionInWork)
        {
            if (transactionInWork.Debet == null || !transactionInWork.Debet.Is("Мои")) return "";

            var periodBefore = new Period(new DateTime(0), transactionInWork.Timestamp.AddSeconds(-1));
            var balanceBefore =
                _accountBalanceCalculator.GetAccountBalanceOnlyForCurrency(transactionInWork.Debet,
                    periodBefore, transactionInWork.Currency);

            return BuildTip(balanceBefore, balanceBefore - transactionInWork.Amount, transactionInWork.Currency);
        }

        public string GetCreditAccountBalance(int selectedTabIndex, Transaction transactionInWork, Transaction relatedTransactionInWork)
        {
            if (selectedTabIndex == 3)
            {
                var periodBefore = new Period(new DateTime(0), transactionInWork.Timestamp.AddSeconds(-1));
                var balanceBefore =
                    _accountBalanceCalculator.GetAccountBalanceOnlyForCurrency(relatedTransactionInWork.Credit,
                        periodBefore, relatedTransactionInWork.Currency);

                return BuildTip(balanceBefore, balanceBefore + relatedTransactionInWork.Amount, relatedTransactionInWork.Currency);
            }

            if (transactionInWork.Operation == OperationType.Доход || transactionInWork.Operation == OperationType.Перенос)
            {
                if (transactionInWork.Credit == null || !transactionInWork.Credit.Is("Мои")) return "";

                var periodBefore = new Period(new DateTime(0), transactionInWork.Timestamp.AddSeconds(-1));
                var balanceBefore =
                    _accountBalanceCalculator.GetAccountBalanceOnlyForCurrency(transactionInWork.Credit,
                        periodBefore, transactionInWork.Currency);

                return BuildTip(balanceBefore, balanceBefore + transactionInWork.Amount, transactionInWork.Currency);
            }
            else return "";
        }
        public string GetExchangeRate(Transaction transactionInWork, Transaction relatedTransactionInWork, int selectedTabIndex)
        {
            return selectedTabIndex == 3
                ? RateDefiner.GetExpression(transactionInWork.Currency, transactionInWork.Amount,
                    relatedTransactionInWork.Currency, relatedTransactionInWork.Amount)
                : "";
        }

    }
}
