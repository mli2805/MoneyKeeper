using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  internal class MonthlyResultDiagramViewModel : Screen
   {
    public MonthlyResultDiagramViewModel(Dictionary<DateTime, decimal> monthlyResults)
    {
      var data = (from pair in monthlyResults
                  select new DiagramPair(pair.Key, (double)pair.Value)).ToList();

      var series = new DiagramSeries
                     {Name = "Сальдо", positiveBrushColor = Brushes.Blue, negativeBrushColor = Brushes.Red, Data = data};

      ModelDataProperty = new List<DiagramSeries> {series};
    }

    public List<DiagramSeries> ModelDataProperty { get; set; }

  }
 
}
