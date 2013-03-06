using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.Utils;

namespace Keeper.DomainModel
{
  public class Deposit : PropertyChangedBase
  {
    private ObservableCollection<string> _report;

    [Import]
    public KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }

    public Account Account { get; set; }
    public DateTime Start { get; set; }
    public DateTime Finish { get; set; }
    public CurrencyCodes MainCurrency { get; set; }
    public decimal StartAmount { get; set; }
    public decimal AdditionalAmounts { get; set; }
    public List<Transaction> Transactions { get; set; }
    public decimal CurrentBalance { get; set; }
    public DepositStates State { get; set; }
    public Brush FontColor
    {
      get
      {
        if (State == DepositStates.Закрыт) return Brushes.Gray;
        if (State == DepositStates.Просрочен) return Brushes.Red;
        return Brushes.Blue;
      }
    }

    public decimal Profit { get; set; }
    public decimal DepositRate { get; set; }
    public decimal Forecast { get; set; }
    public ObservableCollection<string> Report
    {
      get { return _report; }
      set
      {
        if (Equals(value, _report)) return;
        _report = value;
        NotifyOfPropertyChange(() => Report);
      }
    }

    /// <summary>
    /// из предположения, что обратные слэши только в датах, и даты с обеих сторон имеют пробелы
    /// </summary>
    private void ExtractInfoFromName()
    {
      var s = Account.Name;
      var p = s.IndexOf('/');
      var n = s.IndexOf(' ', p);
      Start = Convert.ToDateTime(s.Substring(p - 2, n - p + 2));
      p = s.IndexOf('/', n);
      n = s.IndexOf(' ', p);
      Finish = Convert.ToDateTime(s.Substring(p - 2, n - p + 2));
      p = s.IndexOf('%', n);
      DepositRate = Convert.ToDecimal(s.Substring(n, p - n));
    }

    private void SelectTransactions()
    {
      Transactions = (from transaction in Db.Transactions
                      where transaction.Debet == Account || transaction.Credit == Account
                      orderby transaction.Timestamp
                      select transaction).ToList();
    }

    private void Calculate()
    {
      // шапка отчета
      if (Transactions.Count == 0) return;
      CurrentBalance = Balance.GetBalanceInCurrency(Account, new Period(new DateTime(1001, 1, 1), DateTime.Today.AddDays(1).AddMinutes(-1)), MainCurrency);
      if (CurrentBalance == 0)
      {
        Report.Add("Депозит закрыт. Остаток 0.\n");
        State = DepositStates.Закрыт;
      }
      else
      {
        if (Finish < DateTime.Today)
        {
          Report.Add("!!! Срок депозита истек !!!\n");
          State = DepositStates.Просрочен;
        }
        else
        {
          Report.Add("Действующий депозит.\n");
          State = DepositStates.Открыт;
        }
        var balanceString = MainCurrency != CurrencyCodes.USD ?
              String.Format("{0:#,0} {2}  ($ {1:#,0} )", 
                 CurrentBalance, CurrentBalance / (decimal)Rate.GetLastRate(MainCurrency), MainCurrency.ToString().ToLower()) :
              String.Format("{0:#,0} usd", CurrentBalance);
        Report.Add(String.Format(" Остаток на {0:dd/MM/yyyy} составляет {1} \n", DateTime.Today, balanceString));
      }
      Report.Add("                             Расход                          Доход ");


      // тело отчета
      Profit = 0;
      decimal balanceAfterTransaction = 0;
      var comment = "";
      foreach (var transaction in Transactions)
      {
        var rate = transaction.Currency != CurrencyCodes.USD ? Rate.GetRate(transaction.Currency, transaction.Timestamp) : 1.0;

        if (transaction.Credit == Account)
        {
          if (transaction.Operation == OperationType.Перенос)
          {
            if (balanceAfterTransaction == 0)
            {
              comment = "открытие депозита";
              StartAmount = transaction.Amount;
            }
            else
            {
              comment = "доп взнос";
              AdditionalAmounts += transaction.Amount;
            }
            Profit = Profit - transaction.Amount / (decimal)rate;
          }
          else comment = "начисление процентов";
          balanceAfterTransaction += transaction.Amount;
        }

        if (transaction.Debet == Account)
        {
          Profit = Profit + transaction.Amount / (decimal)rate;
          balanceAfterTransaction -= transaction.Amount;
          comment = balanceAfterTransaction == 0 ? "закрытие депозита" : "частичное снятие";
        }

        Report.Add(String.Format("{0}     {1}", transaction.ToDepositReport(Account), comment));
      }

      // подвал отчета
      if (CurrentBalance != 0)
      {
        var todayRate = MainCurrency != CurrencyCodes.USD ? Rate.GetLastRate(MainCurrency) : 1.0;
        Profit += CurrentBalance / (decimal)todayRate;
      }

      Report.Add(String.Format("\nДоход по депозиту {0:#,0} usd \n", Profit));
      Report.Add(ForecastProfit());
    }

    private string ForecastProfit()
    {
      if (CurrentBalance == 0) return "";

      Forecast = CurrentBalance * DepositRate / 100 * (Finish - Start).Days / 365;
      var forecastInUsd = Forecast;

      if (MainCurrency != CurrencyCodes.USD)
      {
        var todayRate = Rate.GetLastRate(MainCurrency);
        forecastInUsd = forecastInUsd / (decimal)todayRate;
      }
      return String.Format("Прогноз по депозиту {0:#,0} usd", forecastInUsd);
    }

    public void CollectInfo()
    {
      ExtractInfoFromName();
      SelectTransactions();
      Report = new ObservableCollection<string>();
      if (Transactions.Count == 0)
      {
        State = DepositStates.Закрыт;
        return; 
      }
      MainCurrency = Transactions.First().Currency;
      Calculate();
      if (Finish > DateTime.Today) ForecastProfit();
    }

    public decimal GetProfitForYear(int year)
    {
      if (Profit == 0) return 0;
      int startYear = Transactions.First().Timestamp.Year;
      int finishYear = Transactions.Last().Timestamp.AddDays(-1).Year;
      if (year < startYear || year > finishYear) return 0;
      if (startYear == finishYear) return Profit;
      int allDaysCount = (Transactions.Last().Timestamp.AddDays(-1) - Transactions.First().Timestamp).Days;
      if (year == startYear)
      {
        int startYearDaysCount = (new DateTime(startYear, 12, 31) - Transactions.First().Timestamp).Days;
        return Profit*startYearDaysCount/allDaysCount;
      }
      if (year == finishYear)
      {
        int finishYearDaysCount = (Transactions.Last().Timestamp.AddDays(-1) - new DateTime(finishYear, 1, 1)).Days;
        return Profit*finishYearDaysCount/allDaysCount;
      }
      int yearDaysCount = (new DateTime(year, 12, 31) - new DateTime(year, 1, 1)).Days;
      return Profit*yearDaysCount;
    }

  }
}
