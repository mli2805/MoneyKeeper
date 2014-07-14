using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Common;
using Keeper.Utils.Deposits;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
  [Export]
  class MonthForecaster
  {
    private readonly KeeperDb _db;
    private readonly RegularPaymentsProvider _regularPaymentsProvider;
    private readonly RateExtractor _rateExtractor;
    private readonly AccountTreeStraightener _accountTreeStraightener;
    private readonly DepositExtractor _depositExtractor;

    [ImportingConstructor]
    public MonthForecaster(KeeperDb db, RegularPaymentsProvider regularPaymentsProvider, RateExtractor rateExtractor, 
       AccountTreeStraightener accountTreeStraightener, DepositExtractor depositExtractor)
    {
      _db = db;
      _regularPaymentsProvider = regularPaymentsProvider;
      _rateExtractor = rateExtractor;
      _accountTreeStraightener = accountTreeStraightener;
      _depositExtractor = depositExtractor;
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
        s.ForecastRegularIncome.Payments.Add(new EstimatedMoney{Amount = payment.Amount, Currency = payment.Currency, ArticleName = payment.Article});
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
      return (from income in s.Incomes.OnHands.Transactions 
               where income.Article.Name == payment.Article select income).FirstOrDefault() != null;
    }

    private bool CheckExpensePayment(Saldo s, RegularPayment payment)
    {
      return (from tr in _db.Transactions
              where tr.Timestamp.IsMonthTheSame(s.StartDate)
                    && tr.Article != null
                    && tr.Article.Name == payment.Article
              select tr).FirstOrDefault() != null;

    }

    private void CheckDeposits(Saldo s)
    {
      foreach (var account in _accountTreeStraightener.Seek("Депозиты", _db.Accounts).Children)
      {
        if (account.Children.Count != 0) continue;
        var deposit = _depositExtractor.Extract(account);

        if (deposit.CalculatedTotals.EstimatedProcentsInThisMonth == 0) continue;

        s.ForecastRegularIncome.Payments.Add(new EstimatedMoney { Amount = deposit.CalculatedTotals.EstimatedProcentsInThisMonth, 
          ArticleName = string.Format("%%  {0} {1:d MMM}",deposit.Bank ,deposit.FinishDate), 
          Currency = deposit.Currency });

        s.ForecastRegularIncome.EstimatedSum += 
          _rateExtractor.GetUsdEquivalent(deposit.CalculatedTotals.EstimatedProcentsInThisMonth, deposit.Currency, DateTime.Today);
      }
    }

  }
}
