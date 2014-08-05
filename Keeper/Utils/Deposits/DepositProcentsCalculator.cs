using System;
using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel;
using System.Linq;
using Keeper.DomainModel.Deposit;
using Keeper.Utils.Common;

namespace Keeper.Utils.Deposits
{
  [Export]
  public class DepositProcentsCalculator
  {
    private readonly KeeperDb _db;

    [ImportingConstructor]
    public DepositProcentsCalculator(KeeperDb db)
    {
      _db = db;
    }

    public void ProcentsForPeriod(Account account, Period period)
    {
      account.Deposit.CalculatedTotals.ProcentEvaluation = new List<DepositDailyLine>();
      FillinDailyBalances(account, period);
      CalculateDailyProcents(account.Deposit);
      account.Deposit.CalculatedTotals.EstimatedProcentsInThisMonth =
        account.Deposit.CalculatedTotals.ProcentEvaluation.Where(line => line.Date.IsMonthTheSame(DateTime.Today)).Sum(line => line.DayProfit);
      account.Deposit.CalculatedTotals.EstimatedProcents =
        account.Deposit.CalculatedTotals.ProcentEvaluation.Sum(line => line.DayProfit);
    }
    
    private void FillinDailyBalances(Account account, Period period)
    {
      var trs = _db.Transactions.Where(t=>t.Debet.Is(account) || t.Credit.Is(account)).ToList();

      decimal balance = 0; 
      foreach (DateTime day in period)
      {
        var date = day;
        account.Deposit.CalculatedTotals.ProcentEvaluation.Add(new DepositDailyLine { Date = day, Balance = balance });
        balance += trs.Where(t => t.Timestamp.Date <= date.Date && t.Debet.Is(account)).Sum(t => t.Amount);
        balance -= trs.Where(t => t.Timestamp.Date <= date.Date && t.Credit.Is(account)).Sum(t => t.Amount);
      }
    } 

    private void CalculateDailyProcents(Deposit deposit)
    {
      foreach (var line in deposit.CalculatedTotals.ProcentEvaluation)
      {
        line.DepoRate = GetCorrespondingDepoRate(deposit, line.Balance, line.Date);
        line.DayProfit = CalculateOneDayProcents(deposit, line.DepoRate, line.Balance);
      }
    }

    private decimal GetCorrespondingDepoRate(Deposit deposit, decimal balance, DateTime date)
    {
      var line = deposit.DepositOffer.RateLines.LastOrDefault(l => l.AmountFrom <= balance && l.AmountTo >= balance && l.DateFrom < date);
      return line == null ? 0 : line.Rate;
    } 

    private decimal CalculateOneDayProcents(Deposit deposit, decimal depoRate, decimal balance)
    {
//      var year = deposit.IsFactDays ? 365 : 360;
      var year = deposit.DepositOffer.CalculatingRules.IsFactDays ? 365 : 360;
      return balance*depoRate/100/year;
    }
  }
}
