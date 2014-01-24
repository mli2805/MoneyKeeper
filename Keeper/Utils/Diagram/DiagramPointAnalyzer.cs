using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Keeper.Utils.Diagram
{
  class DiagramPointAnalyzer
  {
    private readonly SortedList<DateTime, List<double>> _diagramData;
    private readonly DiagramDrawingCalculator _drawingCalculator;

    public DiagramPointAnalyzer(SortedList<DateTime, List<double>> diagramData, DiagramDrawingCalculator drawingCalculator)
    {
      _diagramData = diagramData;
      _drawingCalculator = drawingCalculator;
    }

    public int PointToBar(Point point, out int leftBar, out bool byHeight)
    {
      leftBar = -1;
      byHeight = false;
      double margin = _drawingCalculator.LeftMargin + _drawingCalculator.Shift / 2 + _drawingCalculator.Gap / 2;
      double d = point.X - margin;
      if (d < 0) return -1; // мышь левее самого левого столбца

      var count = (int)Math.Floor(d / _drawingCalculator.PointPerDataElement);
      var rest = d - count * _drawingCalculator.PointPerDataElement;
      if (rest < _drawingCalculator.PointPerBar && count < _diagramData.Count)
      {
        var barHeight = _drawingCalculator.Y0 - _drawingCalculator.PointPerOneValueAfter * _diagramData.ElementAt(count).Value.Sum();
        if (barHeight < _drawingCalculator.Y0) byHeight = barHeight < point.Y && point.Y < _drawingCalculator.Y0;
        else byHeight = barHeight > point.Y && point.Y > _drawingCalculator.Y0;
        return count; // мышь попала на столбец по горизонтали
      }
      leftBar = count >= _diagramData.Count ? _diagramData.Count - 1 : count;
      return -1; // мышь не попала на столбец по горизонтали, слева кто-то есть
    }

    public int PointToBar(Point point, out int leftBar)
    {
      bool useless;
      return PointToBar(point, out leftBar, out useless);
    }

    public int PointToBar(Point point)
    {
      int useless;
      return PointToBar(point, out useless);
    }


  }
}
