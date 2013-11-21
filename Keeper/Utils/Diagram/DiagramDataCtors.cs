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

    public DiagramData DailyBalancesCtor()
    {
      var allMyMoney = (from account in _db.Accounts where account.Name == "Мои" select account).FirstOrDefault();
      var balances = Calculator.AccountBalancesForPeriodInUsdThirdWay(allMyMoney, new Period(new DateTime(2001, 12, 31), DateTime.Now), Every.Day);
      var data = balances.Select(pair => new DiagramPair(pair.Key, (double) pair.Value)).ToList();

      var dataForDiagram = new List<DiagramSeries>
                             {
                               new DiagramSeries
                                 {
                                   Data = data,
                                   Index = 0,
                                   Name = "Ежедневные остатки",
                                   NegativeBrushColor = Brushes.Red,
                                   PositiveBrushColor = Brushes.Blue
                                 }
                             };

      return new DiagramData {Data = dataForDiagram, Mode = DiagramMode.Line, TimeInterval = Every.Day};
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
        dataForDiagram.Add(new DiagramSeries
            {
              Name = outcome.Name,
              PositiveBrushColor = colorsEnumerator.Current,
              NegativeBrushColor = colorsEnumerator.Current,
              Data = (from pair in Calculator.MonthlyTraffic(outcome.Name)
                      select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
            });
      }

      return new DiagramData { Data = dataForDiagram, Mode = DiagramMode.BarVertical, TimeInterval = Every.Month };
    }

    public DiagramData MonthlyIncomesDiagramCtor()
    {
      var dataForDiagram = new List<DiagramSeries>();

      dataForDiagram.Add(
        new DiagramSeries
        {
          Name = "Зарплата",
          PositiveBrushColor = Brushes.Green,
          NegativeBrushColor = Brushes.Red,
          Index = 0,
          Data = (from pair in Calculator.MonthlyTraffic("Зарплата")
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
        });

      dataForDiagram.Add(
        new DiagramSeries
        {
          Name = "Иррациональные",
          PositiveBrushColor = Brushes.CadetBlue,
          NegativeBrushColor = Brushes.Red,
          Index = 1,
          Data = (from pair in Calculator.MonthlyTraffic("Иррациональные")
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
        });

      dataForDiagram.Add(
        new DiagramSeries
        {
          Name = "Рента",
          PositiveBrushColor = Brushes.Blue,
          NegativeBrushColor = Brushes.Red,
          Index = 2,
          Data = (from pair in Calculator.MonthlyTraffic("Рента")
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
        });

      dataForDiagram.Add(
        new DiagramSeries
        {
          Name = "Подарки",
          PositiveBrushColor = Brushes.DarkOrange,
          NegativeBrushColor = Brushes.Red,
          Index = 3,
          Data = (from pair in Calculator.MonthlyTraffic("Подарки")
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
        });

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
