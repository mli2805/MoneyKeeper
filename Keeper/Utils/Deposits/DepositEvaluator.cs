using System;
using System.Collections.Generic;
using System.Composition;
using System.Windows;
using Keeper.DomainModel;
using System.Linq;

namespace Keeper.Utils.Deposits
{
  public class ProcentEvaluationDailyLine
  {
    public DateTime Date { get; set; }
    public decimal Balance { get; set; }
    public decimal DepoRate { get; set; }
    public decimal DayProfit { get; set; }
  }

  [Export]
  public class DepositEvaluator
  {
    private readonly KeeperDb _db;
    public List<ProcentEvaluationDailyLine> Result { get; set; }

    [ImportingConstructor]
    public DepositEvaluator(KeeperDb db)
    {
      _db = db;
    }

    public decimal ProcentsForPeriod(Account account, Period period)
    {
      Result = new List<ProcentEvaluationDailyLine>();
      FillinBalances(account, period);
      EvaluateProfit(account.Deposit);
      return Result.Sum(line => line.DayProfit);
    }
    
    private void FillinBalances(Account account, Period period)
    {
      var trs = _db.Transactions.Where(t=>t.Debet.Is(account) || t.Credit.Is(account)).ToList();

      decimal balance = 0; 
      foreach (DateTime day in period)
      {
        var date = day;
        balance += trs.Where(t => t.Timestamp.Date == date.Date).Sum(t => t.Amount);
        Result.Add(new ProcentEvaluationDailyLine{Date = day, Balance = balance});
      }
    } 

    private void EvaluateProfit(Deposit deposit)
    {
      foreach (var line in Result)
      {
        line.DepoRate = GetCorrespondingDepoRate(deposit, line.Balance, line.Date);
        line.DayProfit = EvaluateDayProfit(deposit, line.DepoRate, line.Balance);
      }
    }

    private decimal GetCorrespondingDepoRate(Deposit deposit, decimal balance, DateTime date)
    {
      var line = deposit.DepositRateLines.LastOrDefault(l => l.AmountFrom <= balance && l.AmountTo >= balance && l.DateFrom < date);
      return line == null ? 0 : line.Rate;
    } 

    private decimal EvaluateDayProfit(Deposit deposit, decimal depoRate, decimal balance)
    {
      var year = deposit.IsFactDays ? 365 : 360;
      return balance*depoRate/100/year;
    }
  }
}
