using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.Utils;

namespace Keeper.DomainModel
{
  class Deposit : PropertyChangedBase
  {
    private ObservableCollection<string> _report;

    [Import]
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public Account Account { get; set; }
    public DateTime Start { get; set; }
    public DateTime Finish { get; set; }
    public CurrencyCodes MainCurrency { get; set; }
    public List<Transaction> Transactions { get; set; }

    public Decimal Profit { get; set; }
    public Decimal Forecast { get; set; }
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
    private void ExtractDatesFromName()
    {
      var s = Account.Name;
      var p = s.IndexOf('/');
      var n = s.IndexOf(' ', p);
      Start = Convert.ToDateTime(s.Substring(p - 2, n - p + 2));
      p = s.IndexOf('/', n);
      n = s.IndexOf(' ', p);
      Finish = Convert.ToDateTime(s.Substring(p - 2, n - p + 2));
    }

    private void SelectTransactions()
    {
      Transactions = (from transaction in Db.Transactions.Local
                      where transaction.Debet == Account || transaction.Credit == Account
                      orderby transaction.Timestamp
                      select transaction).ToList();
    }

    private void Calculate()
    {
      // шапка отчета
      if (Transactions.Count == 0) return;
      var currentBalance = Balance.GetBalanceInCurrency(Account, new Period(new DateTime(1001, 1, 1), DateTime.Today), MainCurrency);
      if (Finish < DateTime.Today)
        Report.Add(currentBalance == 0 ? "Депозит закрыт. Остаток 0.\n" : "!!! Срок депозита истек !!!\n");
      else
      {
        var balanceString = MainCurrency != CurrencyCodes.USD ?
          String.Format("{0:#,0} byr  ($ {1:#,0} )", currentBalance, currentBalance / (decimal)Rate.GetRate(MainCurrency, DateTime.Today.Date)) :
          String.Format("{0:#,0} usd", currentBalance);
        Report.Add(String.Format("Действующий депозит. Остаток на {0:dd/MM/yyyy} составляет {1} \n", DateTime.Today, balanceString));
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
            comment = balanceAfterTransaction == 0 ? "открытие депозита" : "доп взнос";
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
      if (currentBalance != 0)
      {
        var todayRate = MainCurrency != CurrencyCodes.USD ? Rate.GetRate(MainCurrency, DateTime.Today) : 1.0;
        Profit += currentBalance/(decimal) todayRate;
      }

      Report.Add("");
      Report.Add(String.Format("Доход по депозиту {0:#,0} usd \n", Profit));
      Report.Add("");
    }

    private void ForecastProfit()
    {
    }

    public void MakeReport()
    {
      ExtractDatesFromName();
      SelectTransactions();
      MainCurrency = Transactions.First().Currency;
      Report = new ObservableCollection<string>();
      Calculate();
      if (Finish > DateTime.Today) ForecastProfit();
    }

  }
}
