using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  internal class MonthlyResultDiagramViewModel : Screen
   {
    public MonthlyResultDiagramViewModel(Dictionary<DateTime, decimal> monthlyResults)
    {
      ModelProperty = (from pair in monthlyResults
                       select new DiagramPair(pair.Key, (double)pair.Value)).ToList();
    }

    public List<DiagramPair> ModelProperty { get; set; }

  }
 
}
