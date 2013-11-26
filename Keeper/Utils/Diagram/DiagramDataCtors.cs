using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Keeper.DomainModel;
using Keeper.Utils.Diagram;

namespace Keeper.Utils
{
  class DiagramDataCtors
  {
    private readonly KeeperDb _db;

    private DiagramDataExtractor Extractor { get; set; }

    [ImportingConstructor]
    public DiagramDataCtors(KeeperDb db)
    {
      _db = db;
      Extractor = new DiagramDataExtractor(db);
    }

    public DiagramSeries AccountDailyBalancesToSeries(string name, Brush positiveBrush)
    {
      var balancedAccount = (from account in _db.AccountsPlaneList where account.Name == name select account).FirstOrDefault();
      var balances = Extractor.AccountBalancesForPeriodInUsd(balancedAccount, new Period(new DateTime(2001, 12, 31), DateTime.Now), Every.Day);
      var data = balances.Select(pair => new DiagramPair(pair.Key, (double)pair.Value)).ToList();

      return new DiagramSeries
        {
          Data = data,
          Index = 0,
          Name = name,
          PositiveBrushColor = positiveBrush
        };
    }

    public DiagramData DailyBalancesCtor()
    {
      var dataForDiagram = new List<DiagramSeries>
                             {
                               AccountDailyBalancesToSeries("Депозиты", Brushes.LightSkyBlue),
                               AccountDailyBalancesToSeries("Мои", Brushes.Blue),
                             };

      return new DiagramData { Data = dataForDiagram, Mode = DiagramMode.Line, TimeInterval = Every.Day };
    }

    private Dictionary<DateTime, decimal> ExtractRates(CurrencyCodes currency)
    {
      switch (currency)
      {
        case CurrencyCodes.EUR:
          return _db.CurrencyRates.Where(r => r.Currency == CurrencyCodes.EUR).OrderBy(r => r.BankDay).
                               ToDictionary(currencyRate => currencyRate.BankDay, currencyRate => (decimal)(1 / currencyRate.Rate));
        case CurrencyCodes.BYR:
          return _db.CurrencyRates.Where(r => r.Currency == CurrencyCodes.BYR).OrderBy(r => r.BankDay).
                               ToDictionary(currencyRate => currencyRate.BankDay, currencyRate => (decimal)currencyRate.Rate);
        default:
          return null;
      }
    }

    private Dictionary<DateTime,decimal> FillinGapsInRates(Dictionary<DateTime,decimal> ratesFromDb)
    {
      var result = new Dictionary<DateTime, decimal>();
      var previousPair = new KeyValuePair<DateTime, decimal>(new DateTime(0), 0);
      foreach (var pair in ratesFromDb)
      {
        if (!previousPair.Key.Equals(new DateTime(0)))
        {

          var interval = (pair.Key - previousPair.Key).Days;
          if (interval > 1)
          {
            var delta = (pair.Value - previousPair.Value)/interval;
            for (int i = 1; i < interval; i++)
            {
              result.Add(previousPair.Key.AddDays(i),previousPair.Value + delta*i);
            }
          }
        }
        result.Add(pair.Key,pair.Value);
        previousPair = pair;
      }
      return result;
    }

    public DiagramData RatesCtor(CurrencyCodes currency)
    {
      var rates = FillinGapsInRates(ExtractRates(currency));

      var series = new DiagramSeries
                     {
                       Name = currency.ToString(),
                       PositiveBrushColor = Brushes.Blue,
                       Data = (from pair in rates select new DiagramPair(pair.Key, (double) pair.Value)).ToList()
                     };

      return new DiagramData
               {
                 Data = new List<DiagramSeries>{series},
                 Mode = DiagramMode.Line,
                 TimeInterval = Every.Day
               };
    }

    public DiagramSeries ArticleMonthlyTrafficToSeries(string name, Brush positiveBrush)
    {
      return new DiagramSeries
               {
                 Name = name,
                 PositiveBrushColor = positiveBrush,
                 NegativeBrushColor = positiveBrush,
                 Index = 0,
                 Data = (from pair in Extractor.MonthlyTraffic(name)
                         select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
               };
    }

    public DiagramData MonthlyOutcomesDiagramCtor()
    {
      var outcomes = _db.FindAccountInTree("Все расходы");
      var outcomeColors = new List<Brush> {Brushes.LimeGreen, Brushes.DarkGray, Brushes.OrangeRed, Brushes.Magenta, 
                                           Brushes.Yellow, Brushes.Aquamarine, Brushes.DarkOrange, Brushes.DodgerBlue};
      var colorsEnumerator = outcomeColors.GetEnumerator();

      var dataForDiagram = new List<DiagramSeries>();
      foreach (var outcome in outcomes.Children)
      {
        colorsEnumerator.MoveNext();
        dataForDiagram.Add(ArticleMonthlyTrafficToSeries(outcome.Name, colorsEnumerator.Current));
      }

      return new DiagramData { Data = dataForDiagram, Mode = DiagramMode.BarVertical, TimeInterval = Every.Month };
    }

    public DiagramData MonthlyIncomesDiagramCtor()
    {
      var dataForDiagram = new List<DiagramSeries>
                             {
                               ArticleMonthlyTrafficToSeries("Зарплата",Brushes.Green),
                               ArticleMonthlyTrafficToSeries("Иррациональные",Brushes.CadetBlue),
                               ArticleMonthlyTrafficToSeries("Рента",Brushes.Blue),
                               ArticleMonthlyTrafficToSeries("Подарки",Brushes.DarkOrange),
                             };

      return new DiagramData { Data = dataForDiagram, Mode = DiagramMode.BarVertical, TimeInterval = Every.Month };
    }

    public DiagramData MonthlyResultsDiagramCtor()
    {
      var dataForDiagram = new List<DiagramSeries>
                             {
                               new DiagramSeries
                                 {
                                   Name = "Сальдо",
                                   PositiveBrushColor = Brushes.Blue,
                                   NegativeBrushColor = Brushes.Red,
                                   Index = 0,
                                   Data = (from pair in Extractor.MonthlyResults("Мои")
                                           select new DiagramPair(pair.Key, (double) pair.Value)).ToList()
                                 }
                             };

      return new DiagramData { Data = dataForDiagram, Mode = DiagramMode.BarVertical, TimeInterval = Every.Month };
    }
  }
}
