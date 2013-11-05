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
    public MonthlyResultDiagramViewModel(IEnumerable<DiagramSeries> severalSeries)
    {
      ModelDataProperty = new List<DiagramSeries>();
      foreach (var series in severalSeries)
      {
        ModelDataProperty.Add(series);
        
      }
    }

    public List<DiagramSeries> ModelDataProperty { get; set; }

  }
 
}
