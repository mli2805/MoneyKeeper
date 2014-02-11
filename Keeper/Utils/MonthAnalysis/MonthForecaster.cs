using System;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
  [Export]
  class MonthForecaster
  {
    private readonly KeeperDb _db;
    private readonly Ini _optionSet;
    private readonly RateExtractor _rateExtractor;
    private readonly AccountTreeStraightener _accountTreeStraightener;
    private readonly DepositParser _depositParser;

    [ImportingConstructor]
    public MonthForecaster(KeeperDb db, Ini optionSet, RateExtractor rateExtractor, 
       AccountTreeStraightener accountTreeStraightener, DepositParser depositParser)
    {
      _db = db;
      _optionSet = optionSet;
      _rateExtractor = rateExtractor;
      _accountTreeStraightener = accountTreeStraightener;
      _depositParser = depositParser;
    }

    public void CollectEstimates(Saldo s)
    {
      s.ForecastIncomes = new EstimatedIncomes();
      CheckSalary(s);
      CheckApartmentsToLet(s);
      CheckDeposits(s);
      s.ForecastIncomes.TotalInUsd = s.Incomes.TotalInUsd + s.ForecastIncomes.EstimatedIncomesSum;

    }

    private void CheckSalary(Saldo s)
    {
      decimal estimatedSalaryInEnvelope;

      var tr = (from income in s.Incomes.OnHands.Transactions where income.Article.Name == "Моя з/пл официальная" select income).FirstOrDefault();
      if (tr == null)
      {
        s.ForecastIncomes.Incomes.Add(new EstimatedMoney{Amount = _optionSet.MonthlyTraffic.SalaryCard, ArticleName = "Моя з/пл официальная", Currency = CurrencyCodes.BYR});
        var estimatedSalaryOnCard = _rateExtractor.GetUsdEquivalent(_optionSet.MonthlyTraffic.SalaryCard, CurrencyCodes.BYR, DateTime.Today);
        s.ForecastIncomes.EstimatedIncomesSum += estimatedSalaryOnCard;
        estimatedSalaryInEnvelope = _optionSet.MonthlyTraffic.SalaryFull - estimatedSalaryOnCard;
      }
      else
      {
        estimatedSalaryInEnvelope = _optionSet.MonthlyTraffic.SalaryFull - _rateExtractor.GetUsdEquivalent(tr.Amount, tr.Currency, tr.Timestamp); 
      }

      if ((from income in s.Incomes.OnHands.Transactions where income.Article.Name == "Моя з/пл конверт" select income).FirstOrDefault() == null)
      {
        s.ForecastIncomes.Incomes.Add(new EstimatedMoney { Amount = estimatedSalaryInEnvelope, ArticleName = "Моя з/пл конверт", Currency = CurrencyCodes.USD });
        s.ForecastIncomes.EstimatedIncomesSum += estimatedSalaryInEnvelope;
      }

    }

    private void CheckApartmentsToLet(Saldo s)
    {
      if ((from income in s.Incomes.OnHands.Transactions where income.Article.Name == "Сдача квартиры" select income).FirstOrDefault() != null) return;
      s.ForecastIncomes.Incomes.Add(new EstimatedMoney { Amount = _optionSet.MonthlyTraffic.ApartmentsToLet, ArticleName = "Сдача квартиры", Currency = CurrencyCodes.USD });
      s.ForecastIncomes.EstimatedIncomesSum += _optionSet.MonthlyTraffic.ApartmentsToLet;
    }

    private void CheckDeposits(Saldo s)
    {
      foreach (var account in _accountTreeStraightener.Seek("Депозиты", _db.Accounts).Children)
      {
        if (account.Children.Count != 0) continue;
        var deposit = _depositParser.Analyze(account);
        if (!IsMonthTheSame(deposit.FinishDate, s.StartDate)) continue;
        s.ForecastIncomes.Incomes.Add(new EstimatedMoney { Amount = deposit.EstimatedProcents, 
          ArticleName = string.Format("%%  {0} {1:d MMM}",deposit.Bank ,deposit.FinishDate), 
          Currency = deposit.Currency });
        s.ForecastIncomes.EstimatedIncomesSum += _rateExtractor.GetUsdEquivalent(deposit.EstimatedProcents, deposit.Currency, DateTime.Today);
      }
    }

    private bool IsMonthTheSame(DateTime day1, DateTime day2)
    {
      return (day1.Year == day2.Year && day1.Month == day2.Month);
    }
  }
}
