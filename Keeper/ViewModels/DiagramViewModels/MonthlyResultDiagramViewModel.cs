using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.Utils;

namespace Keeper.ViewModels
{
  class MonthlyResultDiagramViewModel
  {
    public MonthlyResultDiagramViewModel(Dictionary<DateTime, decimal> monthlyResults)
    {
      DiagramDataCtors.AverageMonthlyResults(monthlyResults);
    }
  }
}
