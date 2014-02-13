using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Rates;

namespace Keeper.Utils
{
  [Export]
  public class DepositReporter
  {
    private readonly RateExtractor _rateExtractor;
    private ObservableCollection<string> Report { get; set; }

    [ImportingConstructor]
    public DepositReporter(RateExtractor rateExtractor)
    {
      _rateExtractor = rateExtractor;
      Report = new ObservableCollection<string>();
    }

    public ObservableCollection<string> BuildReport(DepositEvaluations depositEvaluations)
    {
      BuildReportHead(depositEvaluations);
      BuildReportBody(depositEvaluations);
      BuildReportFoot(depositEvaluations);
      return Report;
    }

    private void BuildReportHead(DepositEvaluations depositEvaluations)
    {
      if (depositEvaluations.CurrentBalance == 0) Report.Add("Депозит закрыт. Остаток 0.\n");
      else
      {
        Report.Add(depositEvaluations.DepositCore.FinishDate < DateTime.Today ? "!!! Срок депозита истек !!!\n" : "Действующий депозит.\n");
        var balanceString = depositEvaluations.DepositCore.Currency != CurrencyCodes.USD
                              ? String.Format("{0:#,0} {2}  ($ {1:#,0} )",
                                              depositEvaluations.CurrentBalance,
                                              depositEvaluations.CurrentBalance /
                                              (decimal)_rateExtractor.GetLastRate(depositEvaluations.DepositCore.Currency),
                                              depositEvaluations.DepositCore.Currency.ToString().ToLower())
                              : String.Format("{0:#,0} usd", depositEvaluations.CurrentBalance);
        Report.Add(String.Format(" Остаток на {0:dd/MM/yyyy} составляет {1} \n", DateTime.Today, balanceString));
      }
      Report.Add("                             Расход                          Доход ");
    }

    private void BuildReportBody(DepositEvaluations depositEvaluations)
    {
      var isFirst = true;
      foreach (var operation in depositEvaluations.Traffic)
      {
        string comment = "";
        if (operation.TransactionType == DepositOperations.Явнес) comment = isFirst ? "открытие депозита" : "доп взнос";
        if (operation.TransactionType == DepositOperations.Проценты) comment = "начисление процентов";
        if (operation.TransactionType == DepositOperations.Расход) comment = (depositEvaluations.CurrentBalance == 0 && operation == depositEvaluations.Traffic.Last()) ?
                             "закрытие депозита" : "частичное снятие";

        Report.Add(String.Format("{0}     {1}", TransactionToLineInReport(operation), comment));
        isFirst = false;
      }
    }

    private void BuildReportFoot(DepositEvaluations depositEvaluations)
    {
      Report.Add(String.Format("\nДоход по депозиту {0:#,0} usd \n", depositEvaluations.CurrentProfit));
      if (depositEvaluations.CurrentBalance == 0) return;
      if (depositEvaluations.DepositCore.Currency == CurrencyCodes.USD)
        Report.Add(String.Format("Еще ожидаются проценты {0:#,0} usd", depositEvaluations.EstimatedProcents));
      else Report.Add(String.Format("Еще ожидаются проценты {0:#,0} {1}   (${2:#,0})", depositEvaluations.EstimatedProcents, depositEvaluations.DepositCore.Currency.ToString().ToLower(),
                                                         _rateExtractor.GetUsdEquivalent(depositEvaluations.EstimatedProcents, depositEvaluations.DepositCore.Currency, DateTime.Today)));
      Report.Add(String.Format("\nИтого прогноз по депозиту {0:#,0} usd \n", depositEvaluations.EstimatedProfitInUsd));
    }

    private string TransactionToLineInReport(DepositTransaction transaction)
    {
      var s = transaction.Timestamp.ToString("dd/MM/yyyy");

      var sum = transaction.Currency != CurrencyCodes.USD ?
        String.Format("{0:#,0} {1}  ($ {2:#,0} )",
         transaction.Amount, transaction.Currency.ToString().ToLower(), transaction.Amount / (decimal)_rateExtractor.GetRate(transaction.Currency, transaction.Timestamp)) :
        String.Format("{0:#,0} usd", transaction.Amount);
      if (transaction.TransactionType == DepositOperations.Расход) s = s + "   " + sum;
      else s = s + "                                           " + sum;

      return s;
    }
  }
}
