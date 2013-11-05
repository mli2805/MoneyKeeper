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

  internal class FunctionsWithEvery
  {
    public static bool IsLastDayOf(DateTime date, Every period)
    {
      if (period == Every.Day) return true;
      if (period == Every.Week && date.DayOfWeek == DayOfWeek.Sunday) return true;
      if (period == Every.Month && date.Month != date.AddDays(1).Month) return true;
      if (period == Every.Quarter && date.Month != date.AddDays(1).Month && date.Month%3 == 0) return true;
      if (period == Every.Year && date.Day == 31 && date.Month == 12) return true;
      return false;
    }

    public static DateTime GetLastDayOfTheSamePeriod(DateTime date, Every period)
    {
      if (period == Every.Day) return date;
      if (period == Every.Week)
      {
        while (date.DayOfWeek != DayOfWeek.Sunday) date = date.AddDays(1);
        return date;
      }
      if (period == Every.Month)
      {
        date = date.AddMonths(1);
        return new DateTime(date.Year, date.Month, 1).AddDays(-1);
      }
      if (period == Every.Quarter)
      {
        date = date.AddMonths(3 - (date.Month - 1) % 3);
        return new DateTime(date.Year, date.Month, 1).AddDays(-1);
      }
      if (period == Every.Year) return new DateTime(date.Year +1, 12, 31);
      throw new Exception("Такого периода не бывает!");
    }

    public static int WeekNumber(DateTime date)
    {
      int weekNumber = date.DayOfYear/7;
      if ((int) date.DayOfWeek < date.DayOfYear%7) weekNumber++;
      return weekNumber;
    }

    public static int QuarterNumber(DateTime date)
    {
      return (date.Month - 1)/3 + 1;
    }

    public static bool IsTheSamePeriod(DateTime date1, DateTime date2, Every period)
    {
      if (period == Every.Day) return date1.Date == date2.Date;
      if (period == Every.Week) return WeekNumber(date1) == WeekNumber(date2);
      if (period == Every.Month) return date1.Year == date2.Year && date1.Month == date2.Month;
      if (period == Every.Quarter) return date1.Year == date2.Year && QuarterNumber(date1) == QuarterNumber(date2);
      if (period == Every.Year) return date1.Year == date2.Year;
      
      throw new Exception("Такого периода не бывает!");
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
        if (transaction.Timestamp.Date < period.GetStart()) continue;
        if (transaction.Timestamp.Date > period.GetFinish()) break;

        if ( transaction.Timestamp.Date != currentDate)
        {
          if (!FunctionsWithEvery.IsTheSamePeriod(transaction.Timestamp.Date, currentDate, frequency))
          {
            // последняя подходящая операция в периоде может быть когда угодно, а записываем последним числом периода (чтобы диаграмма красивей была)
            result.Add(FunctionsWithEvery.GetLastDayOfTheSamePeriod(currentDate.Date,frequency), movement); 
            movement = 0;
          }
          currentDate = transaction.Timestamp.Date;
        }


        if (transaction.Article == null || !transaction.Article.IsTheSameOrDescendantOf(kategory)) continue;

        if (transaction.Debet.IsTheSameOrDescendantOf("Мои"))
          movement -= Rate.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
        else
          movement += Rate.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
      }
      result.Add(FunctionsWithEvery.GetLastDayOfTheSamePeriod(currentDate.Date, frequency), movement);
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

      return KategoriesTrafficForPeriodInUsd(kategory, new Period(new DateTime(2002, 1, 1), DateTime.Today), Every.Month); 
      
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
