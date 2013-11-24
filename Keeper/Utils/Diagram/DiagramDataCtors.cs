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

    private DiagramDataCalculation Calculator { get; set; }

    [ImportingConstructor]
    public DiagramDataCtors(KeeperDb db)
    {
      _db = db;
      Calculator = new DiagramDataCalculation(db);
    }

    public DiagramSeries AccountDailyBalancesToSeries(string name, Brush positiveBrush)
    {
      var balancedAccount = (from account in _db.AccountsPlaneList where account.Name == name select account).FirstOrDefault();
      var balances = Calculator.AccountBalancesForPeriodInUsd(balancedAccount, new Period(new DateTime(2001, 12, 31), DateTime.Now), Every.Day);
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

    public DiagramSeries ArticleMonthlyTrafficToSeries(string name, Brush positiveBrush)
    {
      return new DiagramSeries
               {
                 Name = name,
                 PositiveBrushColor = positiveBrush,
                 NegativeBrushColor = positiveBrush,
                 Index = 0,
                 Data = (from pair in Calculator.MonthlyTraffic(name)
                         select new DiagramPair(pair.Key, (double) pair.Value)).ToList()
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
                                   Data = (from pair in Calculator.MonthlyResults("Мои")
                                           select new DiagramPair(pair.Key, (double) pair.Value)).ToList()
                                 }
                             };

      return new DiagramData { Data = dataForDiagram, Mode = DiagramMode.BarVertical, TimeInterval = Every.Month };
    }
  }
}
