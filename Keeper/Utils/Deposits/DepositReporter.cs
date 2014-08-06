using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.DomainModel.Deposit;
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

      if (deposit.CalculationData.CurrentBalance == 0) reportHeader.Add("Депозит закрыт. Остаток 0.\n");
      else
      {
        reportHeader.Add(deposit.FinishDate < DateTime.Today ? "!!! Срок депозита истек !!!" : "Действующий депозит.");
        var balanceString = deposit.DepositOffer.Currency != CurrencyCodes.USD
                              ? String.Format("{0:#,0} {2}  ($ {1:#,0} )",
                                              deposit.CalculationData.CurrentBalance,
                                              deposit.CalculationData.CurrentBalance /
                                              (decimal)_rateExtractor.GetLastRate(deposit.DepositOffer.Currency),
                                              deposit.DepositOffer.Currency.ToString().ToLower())
                              : String.Format("{0:#,0} usd", deposit.CalculationData.CurrentBalance);
        reportHeader.Add(String.Format("Остаток на {0:dd/MM/yyyy} составляет {1} \n", DateTime.Today, balanceString));
      }
       
      reportHeader.Add("    Дата             До операции               Приход                Расход                 После     Примечание");
      return reportHeader;
    }

    public ObservableCollection<DepositReportBodyLine> BuildReportBody(Deposit deposit)
    {
      var reportBody = new ObservableCollection<DepositReportBodyLine>();

      var isFirst = true;
      decimal beforeOperation = 0;
      foreach (var operation in deposit.CalculationData.Traffic)
      {
        var line = new DepositReportBodyLine{Day = operation.Timestamp.Date, BeforeOperation = beforeOperation};
        if (operation.TransactionType == DepositTransactionTypes.Расход)
        {
          line.ExpenseColumn = operation.Amount;
          line.Comment = (deposit.CalculationData.CurrentBalance == 0 && operation == deposit.CalculationData.Traffic.Last())
                           ? "закрытие депозита"
                           : "частичное снятие";
        }
        else
        {
          line.IncomeColumn = operation.Amount;
          if (operation.TransactionType == DepositTransactionTypes.Явнес) line.Comment = isFirst ? "открытие депозита" : "доп взнос";
          if (operation.TransactionType == DepositTransactionTypes.Проценты) line.Comment = "начисление процентов";
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

      reportFooter.Add(String.Format("Доход по депозиту {0:#,0} usd \n", deposit.CalculationData.CurrentProfit));
      if (deposit.CalculationData.CurrentBalance == 0) return reportFooter;
      reportFooter.Add(String.Format("В этом месяце ожидаются проценты {0}", 
        AmountRepresentation(deposit.CalculationData.EstimatedProcentsInThisMonth,deposit.DepositOffer.Currency)));
      reportFooter.Add(String.Format("Всего ожидается процентов {0}", 
        AmountRepresentation(deposit.CalculationData.EstimatedProcents,deposit.DepositOffer.Currency)));
      reportFooter.Add(String.Format("\nИтого прогноз по депозиту {0:#,0} usd", deposit.CalculationData.EstimatedProfitInUsd));
      return reportFooter;
    }

    public string AmountRepresentation(decimal amount, CurrencyCodes currency)
    {
      if (currency == CurrencyCodes.USD)
        return String.Format("{0:#,0} usd", amount);
      return String.Format("{0:#,0} {1}   (${2:#,0})", amount, currency.ToString().ToLower(),
                    _rateExtractor.GetUsdEquivalent(amount, currency, DateTime.Today));
    }

  }
}
