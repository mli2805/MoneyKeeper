using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.MonthAnalysis;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.DbInputOutput;
using Keeper.Utils.DepositProcessing;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
    [Export]
    class MonthForecaster
    {
        private readonly KeeperDb _db;
        private readonly RegularPaymentsProvider _regularPaymentsProvider;
        private readonly RateExtractor _rateExtractor;
        
        private readonly DepositCalculationAggregator _depositCalculationAggregator;

        [ImportingConstructor]
        public MonthForecaster(KeeperDb db, RegularPaymentsProvider regularPaymentsProvider, RateExtractor rateExtractor,
           DepositCalculationAggregator depositCalculationAggregator)
        {
            _db = db;
            _regularPaymentsProvider = regularPaymentsProvider;
            _rateExtractor = rateExtractor;
            
            _depositCalculationAggregator = depositCalculationAggregator;
        }

        public void CollectEstimates(Saldo s)
        {
            s.ForecastRegularIncome = new EstimatedPayments();
            CheckRegularIncome(s);
            CheckRegularExpenses(s);
            CheckDeposits(s);
            s.ForecastRegularIncome.TotalInUsd = s.Incomes.TotalInUsd + s.ForecastRegularIncome.EstimatedSum;
            s.ForecastRegularExpense.TotalInUsd = s.Expense.TotalInUsd + s.ForecastRegularExpense.EstimatedSum;
        }

        private void CheckRegularIncome(Saldo s)
        {
            var regularIncome = _regularPaymentsProvider.RegularPayments.Income;
            foreach (var payment in regularIncome.Where(payment => !CheckIncomePayment(s, payment)))
            {
                s.ForecastRegularIncome.Payments.Add(new EstimatedMoney { Amount = payment.Amount, Currency = payment.Currency, ArticleName = payment.Article });
                s.ForecastRegularIncome.EstimatedSum +=
                 _rateExtractor.GetUsdEquivalent(payment.Amount, payment.Currency, DateTime.Today);
            }
        }

        private void CheckRegularExpenses(Saldo s)
        {
            var regularExpenses = _regularPaymentsProvider.RegularPayments.Expenses;
            foreach (var payment in regularExpenses.Where(payment => !CheckExpensePayment(s, payment)))
            {
                s.ForecastRegularExpense.Payments.Add(new EstimatedMoney { Amount = payment.Amount, Currency = payment.Currency, ArticleName = payment.Article });
            }

        }

        private bool CheckIncomePayment(Saldo s, RegularPayment payment)
        {
            return (from income in s.Incomes.OnHands.Trans
                    where income.Category.Name == payment.Article
                    select income).FirstOrDefault() != null;
        }

        private bool CheckExpensePayment(Saldo s, RegularPayment payment)
        {
            var paymentArticle = _db.SeekAccount(payment.Article);
            return (from tr in _db.TransWithTags
                    where tr.Timestamp.IsMonthTheSame(s.StartDate)
                          && tr.Tags != null
                          && tr.Tags.Contains(paymentArticle)
                    select tr).FirstOrDefault() != null;
        }

        private void CheckDeposits(Saldo s)
        {
            CheckDepositFolder(s, "Депозиты");
            CheckDepositFolder(s, "Карточки");
        }

        private void CheckDepositFolder(Saldo s, string folder)
        {
            foreach (var account in _db.SeekAccount(folder).Children)
            {
                if (account.Children.Count != 0 || !account.IsDeposit()) continue;
//                if (account.Deposit.DepositOffer.Currency == CurrencyCodes.BYR)
//                    account.Deposit.DepositOffer.Currency = CurrencyCodes.BYN;
                _depositCalculationAggregator.FillinFieldsForMonthAnalysis(account.Deposit, s.StartDate);

                if (_rateExtractor.GetUsdEquivalent(account.Deposit.CalculationData.Estimations.ProcentsInThisMonth,account.Deposit.DepositOffer.Currency,DateTime.Today) < 1) continue;

                s.ForecastRegularIncome.Payments.Add(new EstimatedMoney
                {
                    Amount = account.Deposit.CalculationData.Estimations.ProcentsInThisMonth,
                    ArticleName =
                        $"%%  {account.Deposit.ShortName}",
                    Currency = account.Deposit.DepositOffer.Currency
                });

                s.ForecastRegularIncome.EstimatedSum +=
                    _rateExtractor.GetUsdEquivalent(account.Deposit.CalculationData.Estimations.ProcentsInThisMonth,
                        account.Deposit.DepositOffer.Currency, DateTime.Today);
            }
        }
    }
}
