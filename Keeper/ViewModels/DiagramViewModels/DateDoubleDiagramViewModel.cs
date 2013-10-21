using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  class DateDoubleDiagramViewModel : Screen
  {
    public DateDoubleDiagramViewModel(IEnumerable<Dictionary<DateTime, decimal>> diagramDate, int seriesType)
    {
      SeriesType = seriesType;

      DataProperty = new List<List<DiagramPair>>();
      foreach (var dictionary in diagramDate)
      {
        var oneSeries = (from pair in dictionary
                        select new DiagramPair(pair.Key, (double)pair.Value)).ToList();
        
        DataProperty.Add(oneSeries);
      }

    }

    public List<List<DiagramPair>> DataProperty { get; set; }
    public int SeriesType { get; set; }

  }

  
}
