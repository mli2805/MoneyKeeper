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
    public MonthlyResultDiagramViewModel(IEnumerable<DiagramSeries> severalSeries, BarDiagramMode diagramMode)
    {
      ModelDataProperty = new List<DiagramSeries>();
      foreach (var series in severalSeries)
      {
        ModelDataProperty.Add(series);
      }
      ModelModeProperty = diagramMode;
    }

    public List<DiagramSeries> ModelDataProperty { get; set; }
    public BarDiagramMode ModelModeProperty { get; set; }
  }
 
}
