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
    private readonly DepositParser _depositParser;

    [ImportingConstructor]
    public MonthForecaster(KeeperDb db, RegularPaymentsProvider regularPaymentsProvider, RateExtractor rateExtractor, 
       AccountTreeStraightener accountTreeStraightener, DepositParser depositParser)
    {
      _db = db;
      _regularPaymentsProvider = regularPaymentsProvider;
      _rateExtractor = rateExtractor;
      _accountTreeStraightener = accountTreeStraightener;
      _depositParser = depositParser;
    }

    public void CollectEstimates(Saldo s)
    {
      s.ForecastIncomes = new EstimatedIncomes();
      CheckRegularPayments(s);
      CheckDeposits(s);
      s.ForecastIncomes.TotalInUsd = s.Incomes.TotalInUsd + s.ForecastIncomes.EstimatedIncomesSum;
    }

    private void CheckRegularPayments(Saldo s)
    {
      var regularIncome = _regularPaymentsProvider.RegularPayments.Income;

      foreach (var payment in regularIncome)
      {
        if (false) continue;
        s.ForecastIncomes.Incomes.Add(new EstimatedMoney{Amount = payment.Amount, Currency = payment.Currency, ArticleName = payment.Article});
      }
    }

    private void CheckDeposits(Saldo s)
    {
      foreach (var account in _accountTreeStraightener.Seek("Депозиты", _db.Accounts).Children)
      {
        if (account.Children.Count != 0) continue;
        var deposit = _depositParser.Analyze(account);

        if (deposit.Evaluations.EstimatedProcentsInThisMonth == 0) continue;

        s.ForecastIncomes.Incomes.Add(new EstimatedMoney { Amount = deposit.Evaluations.EstimatedProcentsInThisMonth, 
          ArticleName = string.Format("%%  {0} {1:d MMM}",deposit.Bank ,deposit.FinishDate), 
          Currency = deposit.Currency });

        s.ForecastIncomes.EstimatedIncomesSum += 
          _rateExtractor.GetUsdEquivalent(deposit.Evaluations.EstimatedProcentsInThisMonth, deposit.Currency, DateTime.Today);
      }
    }

  }
}
