using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Keeper.Utils.Common;

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
      if (delta < 0)// increase picture
      {
        if (currentSeriesUnited.DiagramData.Count < 6) return false;
      }
      else
      {
      }

      int shiftDateRange = (extremums.MaxDate - extremums.MinDate).Days * delta / 1200;
      if (Math.Abs(shiftDateRange) < 31 && groupInterval == Every.Month) shiftDateRange = 31 * Math.Sign(delta);
      if (Math.Abs(shiftDateRange) < 366 && groupInterval == Every.Year) shiftDateRange = 366 * Math.Sign(delta);
      extremums.MinDate = extremums.MinDate.AddDays(-shiftDateRange);
      extremums.MaxDate = extremums.MaxDate.AddDays(shiftDateRange);
      return true;
    }


    public SortedList<DateTime, List<double>> FindLimitsForRect(SortedList<DateTime, List<double>> currentSeriesUnitedData, DiagramDrawingCalculator drawingCalculator, Point leftTop, Point rightBottom)
    {
      var numberFrom = GetStartBarNumber(currentSeriesUnitedData, drawingCalculator, leftTop);
      var numberTo = GetFinishBarNumber(currentSeriesUnitedData, drawingCalculator, rightBottom);
      if (numberTo - numberFrom < 3) return currentSeriesUnitedData;
      var innerRectCurrentDiagramData = new SortedList<DateTime, List<double>>();
      for (var i = numberFrom; i <= numberTo; i++)
      {
        innerRectCurrentDiagramData.Add(currentSeriesUnitedData.ElementAt(i).Key,
             currentSeriesUnitedData.ElementAt(i).Value);
      }
      return innerRectCurrentDiagramData;
    }

    #region преобразует точки к данным диаграммы
    // привязан к типу диаграммы (в данном случае - столбцовая, время - значение)

    public int GetStartBarNumber(SortedList<DateTime, List<double>> currentSeriesUnitedData, DiagramDrawingCalculator drawingCalculator, Point point)
    {
      var pointAnalyzer = new DiagramPointAnalyzer(currentSeriesUnitedData, drawingCalculator);

      int leftBar;
      var startBarNumber = pointAnalyzer.PointToBar(point, out leftBar);
      if (startBarNumber == -1) startBarNumber = ++leftBar;
      return startBarNumber;
    }

    public int GetFinishBarNumber(SortedList<DateTime, List<double>> currentSeriesUnitedData, DiagramDrawingCalculator drawingCalculator, Point point)
    {
      var pointAnalyzer = new DiagramPointAnalyzer(currentSeriesUnitedData, drawingCalculator);

      int leftBar;
      var finishBarNumber = pointAnalyzer.PointToBar(point, out leftBar);
      if (finishBarNumber == -1) finishBarNumber = leftBar != -1 ? leftBar : 0;
      return finishBarNumber;

    }

    #endregion



  }
}
