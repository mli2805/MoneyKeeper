using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.Utils;

namespace Keeper.DomainModel
{
  class Deposit
  {
    [Import]
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public Account Account { get; set; }
    public DateTime Start { get; set; }
    public DateTime Finish { get; set; }
    public CurrencyCodes MainCurrency { get; set; }
    public List<Transaction> Transactions { get; set; }

    public Decimal Profit { get; set; }
    public Decimal Forecast { get; set; }
    public string Comment { get; set; }

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
      if (Transactions.Count == 0) return;

      Profit = 0;
      foreach (var transaction in Transactions)
      {
        var rate = transaction.Currency != CurrencyCodes.USD ? Rate.GetRate(transaction.Currency, transaction.Timestamp) : 1.0;

        if (transaction.Credit == Account)
        {
          if (transaction.Operation == OperationType.Перенос)
          {
            Comment = Profit == 0 ? "открытие депозита" : "доп взнос";
            Profit = Profit - transaction.Amount / (decimal)rate;
          }
          else Comment = "начисление процентов";
        }

        if (transaction.Debet == Account)
        {
          Profit = Profit + transaction.Amount / (decimal)rate;
          Comment = "снятие со вклада";
        }
      }

      var todayRate = MainCurrency != CurrencyCodes.USD ? Rate.GetRate(MainCurrency, DateTime.Today) : 1.0;
      Profit += Balance.GetBalanceInCurrency(Account, new Period(new DateTime(1001, 1, 1), DateTime.Today), MainCurrency) / (decimal)todayRate;
    }

    public void ForecastProfit()
    {
    }

    public void CollectInfo()
    {
      ExtractDatesFromName();
      SelectTransactions();
      MainCurrency = Transactions.First().Currency;
      Calculate();
      if (Finish > DateTime.Today) ForecastProfit();
    }

  }
}
