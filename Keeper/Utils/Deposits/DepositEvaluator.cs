﻿using System;
using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel;
using System.Linq;
using Keeper.Utils.Common;

namespace Keeper.Utils.Deposits
{
  [Export]
  public class DepositEvaluator
  {
    private readonly KeeperDb _db;

    [ImportingConstructor]
    public DepositEvaluator(KeeperDb db)
    {
      _db = db;
    }

    public void ProcentsForPeriod(Account account, Period period)
    {
      account.Deposit.Evaluations.ProcentEvaluation = new List<ProcentEvaluationDailyLine>();
      FillinBalances(account, period);
      EvaluateProfit(account.Deposit);
      account.Deposit.Evaluations.EstimatedProcentsInThisMonth =
        account.Deposit.Evaluations.ProcentEvaluation.Where(line => line.Date.IsMonthTheSame(DateTime.Today)).Sum(line => line.DayProfit);
      account.Deposit.Evaluations.EstimatedProcents =
        account.Deposit.Evaluations.ProcentEvaluation.Sum(line => line.DayProfit);
    }
    
    private void FillinBalances(Account account, Period period)
    {
      var trs = _db.Transactions.Where(t=>t.Debet.Is(account) || t.Credit.Is(account)).ToList();

      decimal balance = 0; 
      foreach (DateTime day in period)
      {
        var date = day;
        account.Deposit.Evaluations.ProcentEvaluation.Add(new ProcentEvaluationDailyLine { Date = day, Balance = balance });
        balance += trs.Where(t => t.Timestamp.Date == date.Date).Sum(t => t.Amount);
      }
    } 

    private void EvaluateProfit(Deposit deposit)
    {
      foreach (var line in deposit.Evaluations.ProcentEvaluation)
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
//      var year = deposit.IsFactDays ? 365 : 360;
      var year = deposit.ProcentsEvaluated.IsFactDays ? 365 : 360;
      return balance*depoRate/100/year;
    }
  }
}
