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

    public ObservableCollection<string> BuildReport(Deposit deposit)
    {
      BuildReportHead(deposit);
      BuildReportBody(deposit);
      BuildReportFoot(deposit);
      return Report;
    }

    private void BuildReportHead(Deposit deposit)
    {
      if (deposit.CurrentBalance == 0) Report.Add("Депозит закрыт. Остаток 0.\n");
      else
      {
        Report.Add(deposit.Finish < DateTime.Today ? "!!! Срок депозита истек !!!\n" : "Действующий депозит.\n");
        var balanceString = deposit.MainCurrency != CurrencyCodes.USD
                              ? String.Format("{0:#,0} {2}  ($ {1:#,0} )",
                                              deposit.CurrentBalance,
                                              deposit.CurrentBalance /
                                              (decimal)_rateExtractor.GetLastRate(deposit.MainCurrency),
                                              deposit.MainCurrency.ToString().ToLower())
                              : String.Format("{0:#,0} usd", deposit.CurrentBalance);
        Report.Add(String.Format(" Остаток на {0:dd/MM/yyyy} составляет {1} \n", DateTime.Today, balanceString));
      }
      Report.Add("                             Расход                          Доход ");
    }

    private void BuildReportBody(Deposit deposit)
    {
      var isFirst = true;
      foreach (var transaction in deposit.Transactions)
      {
        string comment;
        if (transaction.Credit == deposit.Account)
        {
          if (transaction.Operation == OperationType.Перенос)
            comment = isFirst ? "открытие депозита" : "доп взнос";
          else comment = "начисление процентов";
        }
        else
          comment = (deposit.CurrentBalance == 0 && transaction == deposit.Transactions.Last()) ?
                             "закрытие депозита" : "частичное снятие";

        Report.Add(String.Format("{0}     {1}", TransactionToLineInReport(transaction, deposit), comment));
        isFirst = false;
      }
    }

    private void BuildReportFoot(Deposit deposit)
    {
      Report.Add(String.Format("\nДоход по депозиту {0:#,0} usd \n", deposit.Profit));
      Report.Add(ProfitForecastToLineInReport(deposit));
    }

    private string TransactionToLineInReport(Transaction transaction, Deposit deposit)
    {
      var s = transaction.Timestamp.ToString("dd/MM/yyyy");

      var sum = transaction.Currency != CurrencyCodes.USD ?
        String.Format("{0:#,0} {1}  ($ {2:#,0} )",
         transaction.Amount, transaction.Currency.ToString().ToLower(), transaction.Amount / (decimal)_rateExtractor.GetRate(transaction.Currency, transaction.Timestamp)) :
        String.Format("{0:#,0} usd", transaction.Amount);
      if (transaction.Debet == deposit.Account) s = s + "   " + sum;
      else s = s + "                                           " + sum;

      return s;
    }

    private string ProfitForecastToLineInReport(Deposit deposit)
    {
      if (deposit.CurrentBalance == 0) return "";

      var forecastInUsd = (deposit.MainCurrency != CurrencyCodes.USD) ?
         deposit.Forecast / (decimal)_rateExtractor.GetLastRate(deposit.MainCurrency) :
         deposit.Forecast;

      return String.Format("Прогноз по депозиту {0:#,0} usd", forecastInUsd);
    }

  }
}
