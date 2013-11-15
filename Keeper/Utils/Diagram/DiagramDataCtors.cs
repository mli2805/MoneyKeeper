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
    public IKeeperDb Db { get; private set; }
    public IRate Rate { get; private set; }

    private DiagramDataCalculation Calculator { get; set; }

    [ImportingConstructor]
    public DiagramDataCtors(IKeeperDb db, IRate rate)
    {
      Db = db;
      Rate = rate;

      Calculator = new DiagramDataCalculation(Db, Rate);
    }

    public List<DiagramSeries> MonthlyOutcomesDiagramCtor()
    {
      var dataForDiagram = new List<DiagramSeries>();

      var outcomes = Db.FindAccountInTree("Все расходы");
      var outcomeColors = new List<Brush> {Brushes.Red, Brushes.Turquoise, Brushes.SeaGreen, Brushes.Magenta, 
                                           Brushes.SpringGreen, Brushes.Blue, Brushes.Peru, Brushes.Gold};
      var colorsEnumerator = outcomeColors.GetEnumerator();

      foreach (var outcome in outcomes.Children)
      {
        dataForDiagram.Add(new DiagramSeries
            {
              Name = outcome.Name,
              positiveBrushColor = colorsEnumerator.Current,
              Data = (from pair in Calculator.MonthlyTraffic(outcome.Name)
                      select new DiagramPair(pair.Key, -(double)pair.Value)).ToList()
            });

        colorsEnumerator.MoveNext();
      }

      return dataForDiagram;
    }

    public List<DiagramSeries> MonthlyIncomesDiagramCtor()
    {
      var dataForDiagram = new List<DiagramSeries>();

      dataForDiagram.Add(
        new DiagramSeries
        {
          Name = "Зарплата",
          positiveBrushColor = Brushes.Green,
          negativeBrushColor = Brushes.Red,
          Index = 0,
          Data = (from pair in Calculator.MonthlyTraffic("Зарплата")
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
        });

      dataForDiagram.Add(
        new DiagramSeries
        {
          Name = "Иррациональные",
          positiveBrushColor = Brushes.CadetBlue,
          negativeBrushColor = Brushes.Red,
          Index = 1,
          Data = (from pair in Calculator.MonthlyTraffic("Иррациональные")
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
        });

      dataForDiagram.Add(
        new DiagramSeries
        {
          Name = "Рента",
          positiveBrushColor = Brushes.Blue,
          negativeBrushColor = Brushes.Red,
          Index = 2,
          Data = (from pair in Calculator.MonthlyTraffic("Рента")
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
        });

      dataForDiagram.Add(
        new DiagramSeries
        {
          Name = "Подарки",
          positiveBrushColor = Brushes.DarkOrange,
          negativeBrushColor = Brushes.Red,
          Index = 3,
          Data = (from pair in Calculator.MonthlyTraffic("Подарки")
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList()
        });

      return dataForDiagram;
    }

    public List<DiagramSeries> MonthlyResultsDiagramCtor()
    {
      var dataForDiagram = new List<DiagramSeries>
                             {
                               new DiagramSeries
                                 {
                                   Name = "Сальдо",
                                   positiveBrushColor = Brushes.Blue,
                                   negativeBrushColor = Brushes.Red,
                                   Index = 0,
                                   Data = (from pair in Calculator.MonthlyResults("Мои")
                                           select new DiagramPair(pair.Key, (double) pair.Value)).ToList()
                                 }
                             };

      return dataForDiagram;
    }
  }
}
