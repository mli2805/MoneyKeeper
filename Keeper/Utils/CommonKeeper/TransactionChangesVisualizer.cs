using System;
using System.Composition;
using Keeper.ByFunctional.BalanceEvaluating;
using Keeper.DomainModel;

namespace Keeper.Utils.CommonKeeper
{
    [Export]
    public class TransactionChangesVisualizer
    {
        private const string TemplateForByr = "{0:#,0} {2} -> {1:#,0} {2}";
        private const string TemplateForCurrencies = "{0:#,0.00} {2} -> {1:#,0.00} {2}";
        private readonly AccountBalanceCalculator _accountBalanceCalculator;

        [ImportingConstructor]
        public TransactionChangesVisualizer(AccountBalanceCalculator accountBalanceCalculator)
        {
            _accountBalanceCalculator = accountBalanceCalculator;
        }

        private string BuildTip(decimal before, decimal after, CurrencyCodes currency)
        {
            return currency == CurrencyCodes.BYR
                ? String.Format(TemplateForByr, before, after, currency.ToString().ToLower())
                : String.Format(TemplateForCurrencies, before, after, currency.ToString().ToLower());
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

        public string GetCreditAccountBalance(Transaction transactionInWork, Transaction relatedTransactionInWork)
        {
            if (transactionInWork.IsExchange())
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
    }
}
