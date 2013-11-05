using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  public enum Every
  {
    Day,
    Week,
    Month,
    Quarter,
    Year
  }

  public enum BarDiagramMode
  {
    Horizontal, // столбцы разных серий для одной даты находятся рядом, могут быть отрицательные
    Vertical, // столбцы для одной даты ставятся один на один, не должно быть отрицательных
    Butterfly, // Vertical могут быть положительные и отрицательные серии
    Vertical100 // столбцы для одной даты ставятся один на один и сумма считается за 100%, не должно быть отрицательных
  }

  class FunctionsWithEvery
  {
    public static bool IsLastDayOf(DateTime date, Every period)
    {
      if (period == Every.Day) return true;
      if (period == Every.Week && date.DayOfWeek == DayOfWeek.Sunday) return true;
      if (period == Every.Month && date.Month != date.AddDays(1).Month) return true;
      if (period == Every.Quarter && date.Month != date.AddDays(1).Month && date.Month % 3 == 0) return true;
      if (period == Every.Year && date.Day == 31 && date.Month == 12) return true;
      return false;
    }
  }

  public class DiagramPair
  {
    public DateTime CoorXdate;
    public double CoorYdouble;

    public DiagramPair(DateTime coorXdate, double coorYdouble)
    {
      CoorXdate = coorXdate;
      CoorYdouble = coorYdouble;
    }
  }

  public class DiagramSeries
  {
    public string Name;
    public Brush positiveBrushColor;
    public Brush negativeBrushColor;
    public int Index;
    public List<DiagramPair> Data;
  }

  public class DiagramDate
  {
    public DateTime CoorXdate;
    public List<Double> CoorYdouble;
  }

  public class DateLineDiagramData
  {
    public SortedList<DateTime, List<Double>> DiagramData;
    public int SeriesCount;

    public DateLineDiagramData()
    {
      SeriesCount = 0;
      DiagramData = new SortedList<DateTime, List<double>>();
    }

    public DateLineDiagramData(DateLineDiagramData other)
    {
      SeriesCount = other.SeriesCount;
      DiagramData = new SortedList<DateTime, List<double>>(other.DiagramData);
    }

    public void Add(DiagramSeries series)
    {
      foreach (var pair in series.Data)
      {
        if (!DiagramData.ContainsKey(pair.CoorXdate)) DiagramData.Add(pair.CoorXdate,new List<double>());
        while (DiagramData[pair.CoorXdate].Count < SeriesCount) DiagramData[pair.CoorXdate].Add(0);
        DiagramData[pair.CoorXdate].Add(pair.CoorYdouble);
      }
      SeriesCount++;
    }
  }

  class DiagramDataCtors
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    #region для диаграмм ежедневные остатки и ежедневное распределение по валютам депозитов
    // получение остатка по счету за каждую дату периода [,когда были операции]
    // реализовано не через функцию получения остатка на дату, вызванную для дат периода
    // а за один проход по БД с получением остатков накопительным итогом, т.к. гораздо быстрее

    public static decimal ConvertAllCurrenciesToUsd(Dictionary<CurrencyCodes, decimal> amounts, DateTime date)
    {
      decimal inUsd = 0;
      foreach (var amount in amounts)
      {
        inUsd += Rate.GetUsdEquivalent(amount.Value, amount.Key, date);
      }
      return inUsd;
    }

    public static Dictionary<DateTime, Dictionary<CurrencyCodes, decimal>>
      AccountBalancesForPeriodInCurrencies(Account balancedAccount, Period period)
    {
      var result = new Dictionary<DateTime, Dictionary<CurrencyCodes, decimal>>();
      var balanceInCurrencies = new Dictionary<CurrencyCodes, decimal>();
      var currentDate = period.GetStart();

      foreach (var transaction in Balance.Db.Transactions)
      {
        if (currentDate != transaction.Timestamp.Date)
        {
          result.Add(currentDate, new Dictionary<CurrencyCodes, decimal>(balanceInCurrencies));
          currentDate = currentDate.AddDays(1);
          while (currentDate < transaction.Timestamp.Date)
          {
            //  result.Add(currentDate, balance); // раскомментарить, если даты когда не было изменений тоже должны попадать набор
            currentDate = currentDate.AddDays(1);
          }
        }

        if (transaction.Debet.IsTheSameOrDescendantOf(balancedAccount))
        {
          if (!balanceInCurrencies.ContainsKey(transaction.Currency))
            balanceInCurrencies.Add(transaction.Currency, -transaction.Amount);
          else balanceInCurrencies[transaction.Currency] -= transaction.Amount;
          if (transaction.Amount2 != 0)
          {
            if (!balanceInCurrencies.ContainsKey((CurrencyCodes)transaction.Currency2))
              balanceInCurrencies.Add((CurrencyCodes)transaction.Currency2, transaction.Amount2);
            else balanceInCurrencies[(CurrencyCodes)transaction.Currency2] += transaction.Amount2;
          }
        }

        if (transaction.Credit.IsTheSameOrDescendantOf(balancedAccount))
        {
          if (!balanceInCurrencies.ContainsKey(transaction.Currency)) balanceInCurrencies.Add(transaction.Currency, transaction.Amount);
          else balanceInCurrencies[transaction.Currency] += transaction.Amount;
          if (transaction.Amount2 != 0)
          {
            if (!balanceInCurrencies.ContainsKey((CurrencyCodes)transaction.Currency2))
              balanceInCurrencies.Add((CurrencyCodes)transaction.Currency2, -transaction.Amount2);
            else balanceInCurrencies[(CurrencyCodes)transaction.Currency2] -= transaction.Amount2;
          }
        }

      }
      return result;
    }


    public static Dictionary<DateTime, decimal> KategoriesTrafficForPeriodInUsd(Account kategory, Period period, Every frequency)
    {
      var result = new Dictionary<DateTime, decimal>();
      decimal movement = 0;
      var currentDate = period.GetStart();

      foreach (var transaction in Balance.Db.Transactions)
      {
        if (transaction.Timestamp.Date != currentDate)
        {
          if (FunctionsWithEvery.IsLastDayOf(currentDate, frequency)) { result.Add(currentDate, movement); movement = 0; }
          currentDate = transaction.Timestamp.Date;
        }


        if (transaction.Article == null || !transaction.Article.IsTheSameOrDescendantOf(kategory)) continue;

        if (transaction.Debet.IsTheSameOrDescendantOf("Мои"))
          movement -= Rate.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
        else
          movement += Rate.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
      }

      return result;
    }

    public static Dictionary<DateTime, decimal> AccountBalancesForPeriodInUsdThirdWay(Account balancedAccount, Period period, Every frequency)
    {
      var result = new Dictionary<DateTime, decimal>();
      var balanceInCurrencies = new Dictionary<CurrencyCodes, decimal>();
      var currentDate = period.GetStart();

      foreach (var transaction in Balance.Db.Transactions)
      {
        if (currentDate != transaction.Timestamp.Date)
        {
          if (FunctionsWithEvery.IsLastDayOf(currentDate, frequency)) result.Add(currentDate, ConvertAllCurrenciesToUsd(balanceInCurrencies, currentDate));
          currentDate = currentDate.AddDays(1);
          while (currentDate != transaction.Timestamp.Date)
          {
            // закомментарить часть условия frequency != Every.Day, если надо ежедневно и даты когда не было изменений тоже должны попадать набор
            if (frequency != Every.Day && FunctionsWithEvery.IsLastDayOf(currentDate, frequency)) result.Add(currentDate, ConvertAllCurrenciesToUsd(balanceInCurrencies, currentDate));
            currentDate = currentDate.AddDays(1);
          }
        }

        if (transaction.Debet.IsTheSameOrDescendantOf(balancedAccount))
        {
          if (!balanceInCurrencies.ContainsKey(transaction.Currency)) balanceInCurrencies.Add(transaction.Currency, -transaction.Amount);
          else balanceInCurrencies[transaction.Currency] -= transaction.Amount;
          if (transaction.Amount2 != 0)
          {
            if (!balanceInCurrencies.ContainsKey((CurrencyCodes)transaction.Currency2))
              balanceInCurrencies.Add((CurrencyCodes)transaction.Currency2, transaction.Amount2);
            else balanceInCurrencies[(CurrencyCodes)transaction.Currency2] += transaction.Amount2;
          }
        }

        if (transaction.Credit.IsTheSameOrDescendantOf(balancedAccount))
        {
          if (!balanceInCurrencies.ContainsKey(transaction.Currency)) balanceInCurrencies.Add(transaction.Currency, transaction.Amount);
          else balanceInCurrencies[transaction.Currency] += transaction.Amount;
          if (transaction.Amount2 != 0)
          {
            if (!balanceInCurrencies.ContainsKey((CurrencyCodes)transaction.Currency2))
              balanceInCurrencies.Add((CurrencyCodes)transaction.Currency2, -transaction.Amount2);
            else balanceInCurrencies[(CurrencyCodes)transaction.Currency2] -= transaction.Amount2;
          }
        }
      }
      result.Add(currentDate, ConvertAllCurrenciesToUsd(balanceInCurrencies, currentDate));
      return result;
    }

    #endregion 

    #region для диаграммы ЕЖЕМЕСЯЧНОЕ САЛЬДО

    public static List<DiagramSeries>  MonthlyIncomesDiagramCtor()
    {
      var dataForDiagram = new List<DiagramSeries>();

      dataForDiagram.Add(
        new DiagramSeries
        {
          Name = "Рента",
          positiveBrushColor = Brushes.Blue,
          negativeBrushColor = Brushes.Red,
          Index = 0,
          Data = (from pair in MonthlyTraffic("Рента")
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
        });

      dataForDiagram.Add(
        new DiagramSeries
          {
            Name = "Зарплата",
            positiveBrushColor = Brushes.Green,
            negativeBrushColor = Brushes.Red,
            Index = 0,
            Data = (from pair in MonthlyTraffic("Зарплата")
                    select new DiagramPair(pair.Key, (double) pair.Value)).ToList()
          });

      return dataForDiagram;
    }

    public  static List<DiagramSeries> MonthlyResultsDiagramCtor()
    {
      var dataForDiagram = new List<DiagramSeries>
                             {
                               new DiagramSeries
                                 {
                                   Name = "Сальдо",
                                   positiveBrushColor = Brushes.Blue,
                                   negativeBrushColor = Brushes.Red,
                                   Index = 0,
                                   Data = (from pair in MonthlyResults("Мои")
                                           select new DiagramPair(pair.Key, (double) pair.Value)).ToList()
                                 }
                             };

      return dataForDiagram;
    }

    // на сколько изменился остаток по счету за месяц (разница остатка после Nного и N-1го месяцев)
    // годится именно для счетов, (не для категорий, на которых нет остатка, есть движение)
    public static Dictionary<DateTime, decimal> MonthlyResults(string accountName)
    {
      var result = new Dictionary<DateTime, decimal>();

      var accountForAnalisys = (from account in Db.AccountsPlaneList where account.Name == accountName select account).FirstOrDefault();
      var balances = AccountBalancesForPeriodInUsdThirdWay(accountForAnalisys, new Period(new DateTime(2001, 12, 31), DateTime.Today), Every.Month).
                                                                                                            OrderBy(pair => pair.Key).ToList();

      for (var i = 1; i < balances.Count; i++)
      {
        result.Add(balances[i].Key, balances[i].Value - balances[i-1].Value);
      }
      return result;
    }

    // какие обороты были по счету за месяц
    // применяется для счетов - категорий
    public static Dictionary<DateTime, decimal> MonthlyTraffic(string accountName)
    {
      var kategory = (from account in Db.AccountsPlaneList where account.Name == accountName select account).FirstOrDefault();

      var movements = KategoriesTrafficForPeriodInUsd(kategory, new Period(new DateTime(2001, 12, 31), DateTime.Today), Every.Month).
                                                                                                            OrderBy(pair => pair.Key).ToList();

      return movements.ToDictionary(t => t.Key, t => t.Value);
    }

    private static int MonthsFromStart(DateTime date)
    {
      return (date.Year - 2002)*12 + date.Month;
    }

    public static void AverageMonthlyResults(Dictionary<DateTime, decimal> monthlyResults)
    {
      var averageFromStartDictionary = new Dictionary<DateTime, decimal>();
      var averageFromJanuaryDictionary = new Dictionary<DateTime, decimal>();
      var averageForLast12MonthsDictionary = new Dictionary<DateTime, decimal>();

      decimal averageFromStart = 0;
      decimal averageFromJanuary = 0;
      var last12Months = new SortedDictionary<DateTime, decimal>();
      foreach (var monthSaldo in monthlyResults.OrderBy(pair => pair.Key))
      {
        averageFromStart += monthSaldo.Value;
        averageFromStartDictionary.Add(monthSaldo.Key, Math.Round(averageFromStart / MonthsFromStart(monthSaldo.Key)));

        if (monthSaldo.Key.Month == 1) averageFromJanuary = 0;
        averageFromJanuary += monthSaldo.Value;
        averageFromJanuaryDictionary.Add(monthSaldo.Key, Math.Round(averageFromJanuary / monthSaldo.Key.Month));

        if (last12Months.Count < 12) last12Months.Add(monthSaldo.Key, monthSaldo.Value);
        else
        {
          var minDate = last12Months.Min(pair => pair.Key);
          last12Months.Remove(minDate);
          last12Months.Add(monthSaldo.Key, monthSaldo.Value);
          decimal averageForLast12Months = last12Months.Sum(pair => pair.Value)/12;
          averageForLast12MonthsDictionary.Add(monthSaldo.Key, averageForLast12Months);
        }
      }

    }

    #endregion

  }
}
