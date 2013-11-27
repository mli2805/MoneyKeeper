using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Keeper.Utils.Diagram
{
  class DiagramDataPanAndZoomer
  {
    public bool MoveLimits(DiagramDataSeriesUnited etalonSeriesUnited, DiagramDrawingCalculator calculator, int horizontalPoints, int verticalPoints, ref DiagramDataExtremums extremums)
    {
      var delta = (int)Math.Round(Math.Abs(horizontalPoints / calculator.PointPerDay));
      if (horizontalPoints < 0) // двигаем влево
      {
        if (extremums.MaxDate == etalonSeriesUnited.DiagramData.Last().Key) return false;
        if ((etalonSeriesUnited.DiagramData.Last().Key - extremums.MaxDate).Days < delta)
          delta = (etalonSeriesUnited.DiagramData.Last().Key - extremums.MaxDate).Days;
        extremums.MaxDate = extremums.MaxDate.AddDays(delta);
        extremums.MinDate = extremums.MinDate.AddDays(delta);
      }
      else // вправо
      {
        if (extremums.MinDate == etalonSeriesUnited.DiagramData.ElementAt(0).Key) return false;
        if ((extremums.MinDate - etalonSeriesUnited.DiagramData.ElementAt(0).Key).Days < delta)
          delta = (extremums.MinDate - etalonSeriesUnited.DiagramData.ElementAt(0).Key).Days;
        extremums.MinDate = extremums.MinDate.AddDays(-delta);
        extremums.MaxDate = extremums.MaxDate.AddDays(-delta);
      }
      return true;
    }

    public bool ZoomLimits(DiagramDataSeriesUnited currentSeriesUnited, Every groupInterval, int delta, ref DiagramDataExtremums extremums)
    {
      int shiftDateRange;

      if (delta < 0)// increase picture
      {
        if (currentSeriesUnited.DiagramData.Count < 6) return false;
        shiftDateRange = (extremums.MaxDate - extremums.MinDate).Days * delta / 100;
        if (shiftDateRange < 31 && groupInterval == Every.Month) shiftDateRange = 31;
        if (shiftDateRange < 366 && groupInterval == Every.Year) shiftDateRange = 366;
        extremums.MinDate = extremums.MinDate.AddDays(shiftDateRange);
        extremums.MaxDate = extremums.MaxDate.AddDays(-shiftDateRange);
      }
      else
      {
        shiftDateRange = (extremums.MaxDate - extremums.MinDate).Days * delta / 100;
        if (shiftDateRange < 31 && groupInterval == Every.Month) shiftDateRange = 31;
        if (shiftDateRange < 366 && groupInterval == Every.Year) shiftDateRange = 366;
        extremums.MinDate = extremums.MinDate.AddDays(-shiftDateRange);
        extremums.MaxDate = extremums.MaxDate.AddDays(shiftDateRange);
      }

      return true;
    }


    public void FindLimitsForRect(DiagramDataSeriesUnited currentSeriesUnited, Point leftTop, Point rightBottom, ref DiagramDataExtremums extremums)
    {
//      var numberFrom = GetStartBarNumber(leftTop);
//      var numberTo = GetFinishBarNumber(rightBottom);
//      if (numberTo - numberFrom < 3) return;
//      var nuevoCurrentDiagramData = new SortedList<DateTime, List<double>>();
//      for (var i = numberFrom; i <= numberTo; i++)
//      {
//        nuevoCurrentDiagramData.Add(currentSeriesUnited.DiagramData.ElementAt(i).Key,
//             currentSeriesUnited.DiagramData.ElementAt(i).Value);
//      }
    }

  }
}
