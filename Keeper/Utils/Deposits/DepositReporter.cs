﻿using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Rates;

namespace Keeper.Utils.Deposits
{
  public class DepositReportBodyLine
  {
    public DateTime Day { get; set; }
    public decimal BeforeOperation { get; set; }
    public decimal IncomeColumn { get; set; }
    public decimal ExpenseColumn { get; set; }
    public decimal AfterOperation { get; set; }
    public string Comment { get; set; }
  }

  [Export]
  public class DepositReporter
  {
    private readonly RateExtractor _rateExtractor;
    [ImportingConstructor]
    public DepositReporter(RateExtractor rateExtractor)
    {
      _rateExtractor = rateExtractor;
    }

    public ObservableCollection<string> BuildReportHeader(Deposit deposit)
    {
      var reportHeader = new ObservableCollection<string>();

      if (deposit.Evaluations.CurrentBalance == 0) reportHeader.Add("Депозит закрыт. Остаток 0.\n");
      else
      {
        reportHeader.Add(deposit.FinishDate < DateTime.Today ? "!!! Срок депозита истек !!!\n" : "Действующий депозит.\n");
        var balanceString = deposit.Currency != CurrencyCodes.USD
                              ? String.Format("{0:#,0} {2}  ($ {1:#,0} )",
                                              deposit.Evaluations.CurrentBalance,
                                              deposit.Evaluations.CurrentBalance /
                                              (decimal)_rateExtractor.GetLastRate(deposit.Currency),
                                              deposit.Currency.ToString().ToLower())
                              : String.Format("{0:#,0} usd", deposit.Evaluations.CurrentBalance);
        reportHeader.Add(String.Format(" Остаток на {0:dd/MM/yyyy} составляет {1} \n", DateTime.Today, balanceString));
      }
       
      reportHeader.Add("    Дата             До операции               Приход                Расход                 После     Примечание");
      return reportHeader;
    }

    public ObservableCollection<DepositReportBodyLine> BuildReportBody(Deposit deposit)
    {
      var reportBody = new ObservableCollection<DepositReportBodyLine>();

      var isFirst = true;
      decimal beforeOperation = 0;
      foreach (var operation in deposit.Evaluations.Traffic)
      {
        var line = new DepositReportBodyLine{Day = operation.Timestamp.Date, BeforeOperation = beforeOperation};
        if (operation.TransactionType == DepositOperations.Расход)
        {
          line.ExpenseColumn = operation.Amount;
          line.Comment = (deposit.Evaluations.CurrentBalance == 0 && operation == deposit.Evaluations.Traffic.Last())
                           ? "закрытие депозита"
                           : "частичное снятие";
        }
        else
        {
          line.IncomeColumn = operation.Amount;
          if (operation.TransactionType == DepositOperations.Явнес) line.Comment = isFirst ? "открытие депозита" : "доп взнос";
          if (operation.TransactionType == DepositOperations.Проценты) line.Comment = "начисление процентов";
        }
        beforeOperation = beforeOperation + line.IncomeColumn - line.ExpenseColumn;
        line.AfterOperation = beforeOperation;
//        line.Comment += operation.Comment;
        if (operation.Comment != "") line.Comment = operation.Comment;
        reportBody.Add(line);
        isFirst = false;
      }

      return reportBody;
    }

    public ObservableCollection<string> BuildReportFooter(Deposit deposit)
    {
      var reportFooter = new ObservableCollection<string>();

      reportFooter.Add(String.Format("\nДоход по депозиту {0:#,0} usd \n", deposit.Evaluations.CurrentProfit));
      if (deposit.Evaluations.CurrentBalance == 0) return reportFooter;
      if (deposit.Currency == CurrencyCodes.USD)
        reportFooter.Add(String.Format("Еще ожидаются проценты {0:#,0} usd", deposit.Evaluations.EstimatedProcents));
      else reportFooter.Add(String.Format("Еще ожидаются проценты {0:#,0} {1}   (${2:#,0})", deposit.Evaluations.EstimatedProcents, deposit.Currency.ToString().ToLower(),
                                                         _rateExtractor.GetUsdEquivalent(deposit.Evaluations.EstimatedProcents, deposit.Currency, DateTime.Today)));
      reportFooter.Add(String.Format("\nИтого прогноз по депозиту {0:#,0} usd", deposit.Evaluations.EstimatedProfitInUsd));
      return reportFooter;
    }

  }
}