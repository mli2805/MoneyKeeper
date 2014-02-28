using System;
using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel;
using System.Linq;

namespace Keeper.Utils
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
      Result = new List<ProcentEvaluationDailyLine>();
    }

    public decimal ProcentsForPeriod(Account account, Period period)
    {
      FillinBalances(account, period);
      EvaluateProfit(account.Deposit);
      return Result.Sum(line => line.DayProfit);
    }

    
    private void FillinBalances(Account account, Period period)
    {
      
    }

    private void EvaluateProfit(Deposit deposit)
    {
      foreach (var line in Result)
      {
        line.DepoRate = GetCorrespondingDepoRate(deposit, line.Balance, line.Date);
        line.DayProfit = EvaluateDayProfit(deposit, line.DepoRate, line.Balance, line.Date);
      }
    }

    private decimal GetCorrespondingDepoRate(Deposit deposit, decimal balance, DateTime date)
    {
      
      return 0;
    } 

    private decimal EvaluateDayProfit(Deposit deposit, decimal depoRate, decimal balance, DateTime date)
    {
      return 0;
    }


  }
}
